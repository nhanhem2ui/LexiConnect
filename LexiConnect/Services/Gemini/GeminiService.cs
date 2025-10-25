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
            // Step 1: Upload file to Gemini Files API
            var fileUri = await UploadFileAsync(fileStream, mimeType);

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
                                    mime_type = mimeType,
                                    file_uri = fileUri
                                }
                            }
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

        private async Task<string> UploadFileAsync(Stream fileStream, string mimeType)
        {
            var uploadUrl = $"https://generativelanguage.googleapis.com/upload/v1beta/files?key={_apiKey}";

            using var formContent = new MultipartFormDataContent();

            // Add metadata
            var metadata = new
            {
                file = new
                {
                    display_name = $"upload_{DateTime.UtcNow.Ticks}"
                }
            };
            var metadataJson = JsonSerializer.Serialize(metadata);
            formContent.Add(new StringContent(metadataJson, Encoding.UTF8, "application/json"), "metadata");

            // Add file
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mimeType);
            formContent.Add(streamContent, "file", "upload");

            var response = await _httpClient.PostAsync(uploadUrl, formContent);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonResponse);

            return doc.RootElement
                .GetProperty("file")
                .GetProperty("uri")
                .GetString() ?? throw new Exception("Failed to get file URI");
        }

        private string ExtractTextFromResponse(string jsonResponse)
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