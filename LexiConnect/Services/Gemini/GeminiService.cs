using System.Text;
using System.Text.Json;

namespace LexiConnect.Services.Gemini
{
    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;

        public GeminiService(IConfiguration config, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _apiKey = config["Gemini:ApiKey"]!;
            _model = config["Gemini:Model"] ?? "gemini-2.5-flash";
        }

        public async Task<string> AskQuestionAsync(string question)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = question }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            return ExtractTextFromResponse(jsonResponse);
        }

        public async Task<string> AskQuestionWithFileAsync(string question, Stream fileStream, string mimeType)
        {
            // Step 1: Upload file
            var fileUri = await UploadFileAsync(fileStream, mimeType);

            // Extract the file ID (Gemini expects "files/..." only)
            var filePath = fileUri.Contains("files/")
                ? fileUri.Substring(fileUri.IndexOf("files/"))
                : fileUri;

            // Step 2: Generate content with file reference
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

            var payload = new
            {
                contents = new[]
                {
            new
            {
                parts = new object[]
                {
                    new { text = question },
                    new
                    {
                        file_data = new
                        {
                            mimeType = mimeType,
                            fileUri = fileUri
                        }
                    }
                }
            }
        }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            var jsonResponse = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Gemini generateContent failed: {response.StatusCode} - {jsonResponse}");

            return ExtractTextFromResponse(jsonResponse);
        }


        private async Task<string> UploadFileAsync(Stream fileStream, string mimeType)
        {
            if (fileStream == null || fileStream.Length == 0)
                throw new ArgumentException("File stream is empty.");

            var uploadUrl = $"https://generativelanguage.googleapis.com/upload/v1beta/files?uploadType=multipart&key={_apiKey}";

            //Build multipart/related body manually
            var boundary = "====Boundary" + DateTime.Now.Ticks.ToString("x");
            using var content = new MultipartContent("related", boundary);

            //Metadata must include a "file" object wrapper
            var metadata = new
            {
                file = new
                {
                    display_name = $"upload_{Guid.NewGuid()}"
                }
            };
            var metadataJson = JsonSerializer.Serialize(metadata);
            var metadataContent = new StringContent(metadataJson, Encoding.UTF8, "application/json");
            content.Add(metadataContent);

            //File part
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mimeType);
            content.Add(fileContent);

            //Proper content type for Gemini
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("multipart/related");
            content.Headers.ContentType.Parameters.Add(new System.Net.Http.Headers.NameValueHeaderValue("boundary", boundary));

            var response = await _httpClient.PostAsync(uploadUrl, content);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"File upload failed: {response.StatusCode} - {responseText}");

            using var doc = JsonDocument.Parse(responseText);
            return doc.RootElement.GetProperty("file").GetProperty("uri").GetString()
                   ?? throw new Exception("Failed to retrieve file URI from response.");
        }

        private static string ExtractTextFromResponse(string jsonResponse)
        {
            try
            {
                using var doc = JsonDocument.Parse(jsonResponse);

                // Try to get the text from candidates
                var candidates = doc.RootElement.GetProperty("candidates");
                if (candidates.GetArrayLength() == 0)
                {
                    return "(no response generated)";
                }

                var text = candidates[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return text ?? "(no response)";
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to parse Gemini response: {ex.Message}", ex);
            }
        }
    }
}