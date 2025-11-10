namespace LexiConnect.Services.Quizzes
{
    public interface IQuizGenerationService
    {
        /// <summary>
        /// Generate a quiz using AI based on topic and parameters
        /// </summary>
        Task<QuizGenerationResult> GenerateQuizAsync(QuizGenerationRequest request);
    }

    public class QuizGenerationRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Difficulty { get; set; } = "Medium"; // Easy, Medium, Hard
        public int NumberOfQuestions { get; set; } = 5;
        public int? CourseId { get; set; }
        public int? UniversityId { get; set; }
        public bool IsPublic { get; set; } = false;
        public string? AdditionalContext { get; set; }
    }

    public class QuizGenerationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public BusinessObjects.Quiz? GeneratedQuiz { get; set; }
        public int QuestionsGenerated { get; set; }
    }
}