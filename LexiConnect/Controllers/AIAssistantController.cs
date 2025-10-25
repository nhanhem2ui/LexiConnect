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
        public async Task<IActionResult> AskWithFile([FromForm] string question, [FromForm] IFormFile? file)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(question))
                {
                    return BadRequest(new { error = "Question cannot be empty" });
                }

                string answer;
                var contextualPrompt = BuildContextualPrompt(question);

                if (file != null && file.Length > 0)
                {
                    // Validate file size
                    if (file.Length > _maxFileSize)
                    {
                        return BadRequest(new { error = $"File size exceeds maximum limit of {_maxFileSize / (1024 * 1024)}MB" });
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

                    if (!allowedTypes.Contains(file.ContentType.ToLower()))
                    {
                        return BadRequest(new { error = "File type not supported. Supported types: PDF, Images (JPEG, PNG, GIF, WebP), Text, Word documents" });
                    }

                    // Process with file
                    using var stream = file.OpenReadStream();
                    answer = await _gemini.AskQuestionWithFileAsync(contextualPrompt, stream, file.ContentType);
                }
                else
                {
                    // Process without file
                    answer = await _gemini.AskQuestionAsync(contextualPrompt);
                }

                return Ok(new { answer, timestamp = DateTime.UtcNow, hasFile = file != null });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred processing your request", details = ex.Message });
            }
        }

        private string BuildContextualPrompt(string userQuestion)
        {
            var sb = new StringBuilder();
            sb.AppendLine("You are an AI assistant for LexiConnect, a document sharing platform " +
                "where user uploads their documents in exchanged for points to get other documents.");
            sb.AppendLine("Your role is to help users with:");
            sb.AppendLine("- Answering study questions and academic topics");
            sb.AppendLine("- Finding and navigating documents and resources");
            sb.AppendLine("- Providing information about how to contact support");
            sb.AppendLine("- General guidance about using the platform");
            sb.AppendLine();
            sb.AppendLine("Platform Information:");
            sb.AppendLine("- Support Email: support@lexiconnect.com");
            sb.AppendLine("- Documents can be found in the Resources section at /resources");
            sb.AppendLine("- Technical Support: Use the chat support button in the bottom right");
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
}