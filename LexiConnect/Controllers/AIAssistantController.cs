using BusinessObjects;
using LexiConnect.Services.Gemini;
using LexiConnect.Services.Quizzes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Services;
using System.Security.Claims;
using System.Text;

namespace LexiConnect.Controllers
{
    [Authorize]
    public class AIAssistantController : Controller
    {
        private readonly IGeminiService _gemini;
        private readonly IWebHostEnvironment _env;
        private readonly IGenericService<AIUsageLimit> _usageLimitService;
        private readonly IQuizGenerationService _quizGenerationService;
        private readonly IGenericService<Quiz> _quizService;
        private readonly IGenericService<Users> _userService;
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB

        public AIAssistantController(
            IGeminiService gemini,
            IWebHostEnvironment env,
            IGenericService<AIUsageLimit> usageLimitService,
            IQuizGenerationService quizGenerationService,
            IGenericService<Quiz> quizService,
            IGenericService<Users> userService)
        {
            _gemini = gemini;
            _env = env;
            _usageLimitService = usageLimitService;
            _quizService = quizService;
            _quizGenerationService = quizGenerationService;
            _userService = userService;
        }

        public async Task<IActionResult> Question()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Auth");

            // Get usage stats for display
            var stats = await GetUsageStatsAsync(userId);
            ViewBag.UsageStats = stats;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetUsageStats()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var stats = await GetUsageStatsAsync(userId);

            return Ok(new
            {
                remaining = stats.Remaining,
                used = stats.Used,
                total = stats.Total,
                isUnlimited = stats.Total == null
            });
        }

        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] AskRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { error = "User not authenticated" });

                if (string.IsNullOrWhiteSpace(request.Question))
                {
                    return BadRequest(new { error = "Question cannot be empty" });
                }

                // Check AI usage limit
                var (canUse, message, _) = await CanUseAIAsync(userId);
                if (!canUse)
                {
                    return StatusCode(429, new { error = message, limitReached = true });
                }

                // Record usage
                await RecordUsageAsync(userId);

                var contextualPrompt = BuildContextualPrompt(request.Question);
                var answer = await _gemini.AskQuestionAsync(contextualPrompt);

                // Get updated stats
                var stats = await GetUsageStatsAsync(userId);

                return Ok(new
                {
                    answer,
                    timestamp = DateTime.UtcNow,
                    usageStats = new
                    {
                        remaining = stats.Remaining,
                        used = stats.Used,
                        total = stats.Total
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred processing your request", details = ex.Message });
            }
        }

        [HttpPost]
        [RequestSizeLimit(5 * 1024 * 1024)] // 5MB limit
        [RequestFormLimits(MultipartBodyLengthLimit = 5 * 1024 * 1024)]
        public async Task<IActionResult> AskWithFile(AskRequestWithFile askRequest)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Content("{\"error\": \"User not authenticated\"}", "application/json");

                if (string.IsNullOrWhiteSpace(askRequest.Question))
                {
                    return Content("{\"error\": \"Question cannot be empty\"}", "application/json");
                }

                // Check AI usage limit
                var (canUse, message, _) = await CanUseAIAsync(userId);
                if (!canUse)
                {
                    return Content($"{{\"error\": \"{message}\", \"limitReached\": true}}", "application/json");
                }

                string answer;
                var contextualPrompt = BuildContextualPrompt(askRequest.Question);

                if (askRequest.File != null && askRequest.File.Length > 0)
                {
                    // Validate file size
                    if (askRequest.File.Length > _maxFileSize)
                    {
                        return Content($"{{\"error\": \"File size exceeds maximum limit of {_maxFileSize / (1024 * 1024)}MB\"}}", "application/json");
                    }

                    // Validate file type
                    var allowedTypes = new[] {
                        "application/pdf",
                        "image/jpeg",
                        "image/png",
                        "image/gif",
                        "image/webp",
                        "text/plain",
                        "application/msword",
                        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                    };

                    if (!allowedTypes.Contains(askRequest.File.ContentType.ToLower()))
                    {
                        return Content("{\"error\": \"File type not supported. Supported types: PDF, Images (JPEG, PNG, GIF, WebP), Text, Word documents\"}", "application/json");
                    }

                    // Process with file
                    using var stream = askRequest.File.OpenReadStream();
                    answer = await _gemini.AskQuestionWithFileAsync(contextualPrompt, stream, askRequest.File.ContentType);
                }
                else
                {
                    // Process without file
                    answer = await _gemini.AskQuestionAsync(contextualPrompt);
                }

                // Record usage AFTER successful response
                await RecordUsageAsync(userId);

                // Get updated stats
                var stats = await GetUsageStatsAsync(userId);

                return Ok(new
                {
                    answer,
                    timestamp = DateTime.UtcNow,
                    hasFile = askRequest.File != null,
                    usageStats = new
                    {
                        remaining = stats.Remaining,
                        used = stats.Used,
                        total = stats.Total
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return Content($"{{\"error\": \"An error occurred: {ex.Message}\"}}", "application/json");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GenerateQuiz([FromBody] GenerateQuizRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { error = "User not authenticated" });

                // Check AI usage limit
                var (canUse, message, _) = await CanUseAIAsync(userId);
                if (!canUse)
                {
                    return StatusCode(429, new { error = message, limitReached = true });
                }

                // Record usage
                await RecordUsageAsync(userId);

                // Generate quiz
                var quizRequest = new QuizGenerationRequest
                {
                    UserId = userId,
                    Title = request.Title,
                    Description = request.Description,
                    Subject = request.Subject,
                    Difficulty = request.Difficulty ?? "Medium",
                    NumberOfQuestions = request.NumberOfQuestions > 0 ? request.NumberOfQuestions : 5,
                    CourseId = request.CourseId,
                    UniversityId = request.UniversityId,
                    IsPublic = request.IsPublic,
                    AdditionalContext = request.AdditionalContext
                };

                var result = await _quizGenerationService.GenerateQuizAsync(quizRequest);

                if (!result.Success)
                {
                    return StatusCode(500, new { error = result.Message });
                }

                // Get updated stats
                var stats = await GetUsageStatsAsync(userId);

                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    quizId = result.GeneratedQuiz?.QuizId,
                    questionsGenerated = result.QuestionsGenerated,
                    usageStats = new
                    {
                        remaining = stats.Remaining,
                        used = stats.Used,
                        total = stats.Total
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred generating the quiz", details = ex.Message });
            }
        }

        private string BuildContextualPrompt(string userQuestion)
        {
            var sb = new StringBuilder();
            sb.AppendLine("You are an AI assistant for LexiConnect, a document sharing platform " +
                "where users upload their documents in exchange for points to get other documents.");
            sb.AppendLine("Your role is to help users with:");
            sb.AppendLine("- Answering study questions and academic topics");
            sb.AppendLine("- Finding and navigating documents and resources");
            sb.AppendLine("- Providing information about how to contact support");
            sb.AppendLine("- General guidance about using the platform");
            sb.AppendLine();

            string supportChatUrl = Url.Action("Chat", "Chat", new { id = "A07176C1-5B1F-499B-980B-793005380E1C" }, Request.Scheme) ?? string.Empty;
            string resetPasswordUrl = Url.Action("ResetPassword", "Auth", Request.Scheme) ?? string.Empty;
            string universitiesUrl = Url.Action("AllUniversities", "University", Request.Scheme) ?? string.Empty;

            sb.AppendLine("Platform Information:");
            sb.AppendLine("- Support Email: lexiconnect@support.com");
            sb.AppendLine("- Documents can be found in the Resources section at /Documents");
            sb.AppendLine($"- Technical Support: Use the chat support button in the bottom right, or [click here to contact support]({supportChatUrl}).");
            sb.AppendLine($"- Reset password by [click here to reset password]({resetPasswordUrl}).");
            sb.AppendLine($"- About universities's documents are available at LexiConnect, feel free by browsing yourself, [click here]({universitiesUrl}).");
            sb.AppendLine();
            sb.AppendLine($"User Question: {userQuestion}");
            sb.AppendLine();
            sb.AppendLine("Provide a helpful, concise, and friendly response.");

            return sb.ToString();
        }

        public async Task<(bool CanUse, string Message, AIUsageLimit Limit)> CanUseAIAsync(string userId)
        {
            string currentMonth = DateTime.UtcNow.ToString("yyyy-MM");

            var limit = await _usageLimitService
                .GetAllQueryable(l => l.UserId == userId && l.MonthYear == currentMonth)
                .Include(l => l.User)
                .FirstOrDefaultAsync();

            var user = limit?.User ?? await _userService.GetAsync(u => u.Id == userId);
            if (user == null)
                return (false, "User not found", null!);

            bool isPremium = user.SubscriptionPlanId == 2;
            int? expectedLimit = isPremium ? null : 10;

            if (limit == null)
            {
                limit = new AIUsageLimit
                {
                    UserId = userId,
                    MonthYear = currentMonth,
                    UsedThisMonth = 0,
                    MonthlyLimit = expectedLimit,
                    LastResetDate = DateTime.UtcNow
                };
                await _usageLimitService.AddAsync(limit);
            }
            else
            {
                bool needsUpdate = false;

                var lastReset = limit.LastResetDate ?? DateTime.UtcNow.AddMonths(-1);
                if (lastReset.Year != DateTime.UtcNow.Year || lastReset.Month != DateTime.UtcNow.Month)
                {
                    limit.UsedThisMonth = 0;
                    limit.LastResetDate = DateTime.UtcNow;
                    needsUpdate = true;
                }

                if (limit.MonthlyLimit != expectedLimit)
                {
                    limit.MonthlyLimit = expectedLimit;
                    needsUpdate = true;
                }

                if (needsUpdate)
                    await _usageLimitService.UpdateAsync(limit);
            }

            if (limit.MonthlyLimit == null)
                return (true, "Unlimited usage", limit);

            if (limit.UsedThisMonth >= limit.MonthlyLimit.Value)
                return (false, $"Monthly AI limit reached ({limit.MonthlyLimit} requests). Upgrade to Premium for unlimited access.", limit);

            int remaining = limit.MonthlyLimit.Value - limit.UsedThisMonth;
            return (true, $"{remaining} requests remaining this month", limit);
        }

        public async Task<bool> RecordUsageAsync(string userId)
        {
            // Get the tracked AIUsageLimit from CanUseAIAsync
            var (canUse, _, limit) = await CanUseAIAsync(userId);

            if (limit == null)
                return false;

            limit.UsedThisMonth++;
            limit.LastUsedAt = DateTime.UtcNow;
            await _usageLimitService.UpdateAsync(limit);

            return true;
        }


        public async Task<(int? Remaining, int Used, int? Total)> GetUsageStatsAsync(string userId)
        {
            string currentMonth = DateTime.UtcNow.ToString("yyyy-MM");

            var limit = await _usageLimitService.GetAsync(l => l.UserId == userId && l.MonthYear == currentMonth);

            if (limit == null)
            {
                // Initialize usage record if missing
                await CanUseAIAsync(userId);
                limit = await _usageLimitService.GetAsync(l => l.UserId == userId && l.MonthYear == currentMonth);
            }

            if (limit == null)
                return (10, 0, 10); // default free tier

            int? remaining = limit.MonthlyLimit.HasValue ? limit.MonthlyLimit.Value - limit.UsedThisMonth : null;
            return (remaining, limit.UsedThisMonth, limit.MonthlyLimit);
        }

        public class AskRequest
        {
            public string Question { get; set; } = string.Empty;
        }

        public class AskRequestWithFile
        {
            public string Question { get; set; } = string.Empty;
            public IFormFile? File { get; set; }
        }

        public class GenerateQuizRequest
        {
            public string Title { get; set; } = string.Empty;
            public string? Description { get; set; }
            public string Subject { get; set; } = string.Empty;
            public string? Difficulty { get; set; }
            public int NumberOfQuestions { get; set; } = 5;
            public int? CourseId { get; set; }
            public int? UniversityId { get; set; }
            public bool IsPublic { get; set; } = false;
            public string? AdditionalContext { get; set; }
        }
    }
}