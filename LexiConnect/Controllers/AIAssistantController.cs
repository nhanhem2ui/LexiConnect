using LexiConnect.Services.Gemini;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace LexiConnect.Controllers
{
    [Authorize]
    public class AIAssistantController : Controller
    {
        private readonly IGeminiService _gemini;
        private readonly IWebHostEnvironment _env;
        private readonly long _maxFileSize = 10 * 1024 * 1024; // 10MB

        public AIAssistantController(IGeminiService gemini, IWebHostEnvironment env)
        {
            _gemini = gemini;
            _env = env;
        }

        public IActionResult Question()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] AskRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Question))
                {
                    return BadRequest(new { error = "Question cannot be empty" });
                }

                var contextualPrompt = BuildContextualPrompt(request.Question);
                var answer = await _gemini.AskQuestionAsync(contextualPrompt);

                return Ok(new { answer, timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred processing your request", details = ex.Message });
            }
        }

        [HttpPost]
        [RequestSizeLimit(10 * 1024 * 1024)] // 10MB limit
        [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
        public async Task<IActionResult> AskWithFile(AskRequestWithFile askRequest)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(askRequest.Question))
                {
                    return Content("{\"error\": \"Question cannot be empty\"}", "application/json");
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

                return Ok(new { answer, timestamp = DateTime.UtcNow, hasFile = askRequest.File != null });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return Content($"{{\"error\": \"An error occurred: {ex.Message}\"}}", "application/json");
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
}