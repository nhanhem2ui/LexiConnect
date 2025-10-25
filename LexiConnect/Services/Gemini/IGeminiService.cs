namespace LexiConnect.Services.Gemini
{
    public interface IGeminiService
    {
        Task<string> AskQuestionAsync(string question);
        Task<string> AskQuestionWithFileAsync(string question, Stream fileStream, string mimeType);
    }
}
