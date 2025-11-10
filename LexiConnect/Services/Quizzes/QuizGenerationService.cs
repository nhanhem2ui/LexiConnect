using BusinessObjects;
using LexiConnect.Services.Gemini;
using Services;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LexiConnect.Services.Quizzes
{
    public class QuizGenerationService : IQuizGenerationService
    {
        private readonly IGeminiService _gemini;
        private readonly IGenericService<Quiz> _quizService;

        public QuizGenerationService(IGeminiService gemini, IGenericService<Quiz> quizService)
        {
            _gemini = gemini;
            _quizService = quizService;
        }

        public async Task<QuizGenerationResult> GenerateQuizAsync(QuizGenerationRequest request)
        {
            try
            {
                // Build prompt for AI
                var prompt = BuildQuizPrompt(request);

                // Call Gemini API
                var aiResponse = await _gemini.AskQuestionAsync(prompt);

                // Parse AI response into quiz structure
                var quiz = ParseQuizResponse(aiResponse, request);

                if (quiz == null || quiz.Questions.Count == 0)
                {
                    return new QuizGenerationResult
                    {
                        Success = false,
                        Message = "Failed to generate quiz questions from AI response",
                        QuestionsGenerated = 0
                    };
                }

                // Save to database
                await _quizService.AddAsync(quiz);

                return new QuizGenerationResult
                {
                    Success = true,
                    Message = $"Successfully generated quiz with {quiz.Questions.Count} questions",
                    GeneratedQuiz = quiz,
                    QuestionsGenerated = quiz.Questions.Count
                };
            }
            catch (Exception ex)
            {
                return new QuizGenerationResult
                {
                    Success = false,
                    Message = $"Error generating quiz: {ex.Message}",
                    QuestionsGenerated = 0
                };
            }
        }

        private static string BuildQuizPrompt(QuizGenerationRequest request)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Generate a multiple-choice quiz with the following specifications:");
            sb.AppendLine($"- Subject: {request.Subject}");
            sb.AppendLine($"- Difficulty: {request.Difficulty}");
            sb.AppendLine($"- Number of Questions: {request.NumberOfQuestions}");

            if (!string.IsNullOrWhiteSpace(request.AdditionalContext))
            {
                sb.AppendLine($"- Additional Context: {request.AdditionalContext}");
            }

            sb.AppendLine();
            sb.AppendLine("IMPORTANT: Respond ONLY with valid JSON in this exact format, no markdown, no preamble:");
            sb.AppendLine(@"{
              ""questions"": [
                {
                  ""questionText"": ""Question here?"",
                  ""questionType"": ""MultipleChoice"",
                  ""optionA"": ""First option"",
                  ""optionB"": ""Second option"",
                  ""optionC"": ""Third option"",
                  ""optionD"": ""Fourth option"",
                  ""correctAnswer"": ""A"",
                  ""explanation"": ""Brief explanation"",
                  ""points"": 1
                }
              ]
            }");

            sb.AppendLine();
            sb.AppendLine("Requirements:");
            sb.AppendLine("- Each question must have at least 2 options and at max 4 options (A, B, C, D)");
            sb.AppendLine("- correctAnswer must be one of: A, B, C, or D");
            sb.AppendLine("- Include brief explanations for correct answers");
            sb.AppendLine("- Questions should be clear and unambiguous");
            sb.AppendLine($"- Difficulty level: {request.Difficulty}");

            return sb.ToString();
        }

        private static Quiz? ParseQuizResponse(string aiResponse, QuizGenerationRequest request)
        {
            try
            {
                // Clean response - remove markdown code blocks if present
                var cleaned = aiResponse.Trim();
                cleaned = Regex.Replace(cleaned, @"^```json\s*", "", RegexOptions.Multiline);
                cleaned = Regex.Replace(cleaned, @"```\s*$", "", RegexOptions.Multiline);
                cleaned = cleaned.Trim();

                // Parse JSON
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var quizData = JsonSerializer.Deserialize<QuizResponseModel>(cleaned, options);

                if (quizData?.Questions == null || quizData.Questions.Count == 0)
                    return null;

                // Create quiz entity
                var quiz = new BusinessObjects.Quiz
                {
                    CreatorId = request.UserId,
                    Title = request.Title,
                    Description = request.Description,
                    Subject = request.Subject,
                    Difficulty = request.Difficulty,
                    CourseId = request.CourseId,
                    UniversityId = request.UniversityId,
                    IsPublic = request.IsPublic,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                // Add questions
                int order = 1;
                foreach (var q in quizData.Questions)
                {
                    var question = new QuizQuestion
                    {
                        QuestionOrder = order++,
                        QuestionText = q.QuestionText,
                        QuestionType = q.QuestionType ?? "MultipleChoice",
                        OptionA = q.OptionA,
                        OptionB = q.OptionB,
                        OptionC = q.OptionC,
                        OptionD = q.OptionD,
                        CorrectAnswer = q.CorrectAnswer.ToUpper(),
                        Explanation = q.Explanation,
                        Points = q.Points > 0 ? q.Points : 1,
                        CreatedAt = DateTime.UtcNow
                    };

                    quiz.Questions.Add(question);
                }

                return quiz;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing quiz response: {ex.Message}");
                Console.WriteLine($"Response was: {aiResponse}");
                return null;
            }
        }

        // Models for JSON deserialization
        private class QuizResponseModel
        {
            public List<QuestionModel> Questions { get; set; } = new();
        }

        private class QuestionModel
        {
            public string QuestionText { get; set; } = string.Empty;
            public string? QuestionType { get; set; }
            public string OptionA { get; set; } = string.Empty;
            public string OptionB { get; set; } = string.Empty;
            public string? OptionC { get; set; }
            public string? OptionD { get; set; }
            public string CorrectAnswer { get; set; } = string.Empty;
            public string? Explanation { get; set; }
            public int Points { get; set; } = 1;
        }
    }
}