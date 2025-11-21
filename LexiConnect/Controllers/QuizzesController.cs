using BusinessObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Services;
using System.Security.Claims;

namespace LexiConnect.Controllers
{
    [Authorize]
    public class QuizzesController : Controller
    {
        private readonly IGenericService<Quiz> _quizService;
        private readonly IGenericService<QuizQuestion> _quizQuestionService;

        public QuizzesController(
            IGenericService<Quiz> quizService,
            IGenericService<QuizQuestion> quizQuestionService)
        {
            _quizService = quizService;
            _quizQuestionService = quizQuestionService;
        }

        /// <summary>
        /// Display the current user's quizzes
        /// </summary>
        public async Task<IActionResult> UserQuizzes()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Auth");

            var quizzes = _quizService.GetAllQueryable(q => q.CreatorId == userId);
            var ordered = await quizzes
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            return View(ordered);
        }

        /// <summary>
        /// Display all public quizzes from all users
        /// </summary>
        [AllowAnonymous] // Allow non-authenticated users to browse public quizzes
        public async Task<IActionResult> PublicQuizzes()
        {
            var publicQuizzes = _quizService.GetAllQueryable(q => q.IsPublic);

            // Include creator information if you have navigation property set up
            var ordered = await publicQuizzes
                .Include(q => q.Creator) // Include creator info if navigation property exists
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            return View(ordered);
        }

        /// <summary>
        /// Take a quiz (for both personal and public quizzes)
        /// </summary>
        public async Task<IActionResult> TakeQuiz(int id)
        {
            var quiz = await _quizService.GetAsync(q => q.QuizId == id);

            if (quiz == null)
                return NotFound();

            // Check if user has permission to take this quiz
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Allow if quiz is public OR if user is the creator
            if (!quiz.IsPublic && quiz.CreatorId != userId)
            {
                TempData["Error"] = "You don't have permission to access this quiz.";
                return RedirectToAction("PublicQuizzes");
            }

            var questions = _quizQuestionService.GetAllQueryable(q => q.QuizId == quiz.QuizId);

            var quizSession = new QuizSessionViewModel
            {
                QuizId = quiz.QuizId,
                Title = quiz.Title,
                Description = quiz.Description,
                Subject = quiz.Subject,
                Difficulty = quiz.Difficulty,
                Questions = questions.OrderBy(q => q.QuestionOrder).ToList()
            };

            return View(quizSession);
        }

        /// <summary>
        /// API endpoint to submit quiz results and get score
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SubmitQuiz(int quizId, Dictionary<int, string> answers)
        {
            var quiz = await _quizService.GetAsync(q => q.QuizId == quizId);
            if (quiz == null)
                return NotFound();

            var questions = _quizQuestionService.GetAllQueryable(q => q.QuizId == quizId).ToList();

            int correctCount = 0;
            foreach (var question in questions)
            {
                if (answers.ContainsKey(question.QuestionId) &&
                    answers[question.QuestionId] == question.CorrectAnswer)
                {
                    correctCount++;
                }
            }

            var result = new
            {
                correct = correctCount,
                total = questions.Count,
                percentage = (correctCount * 100.0 / questions.Count)
            };

            return Json(result);
        }
    }

    /// <summary>
    /// ViewModel for quiz session
    /// </summary>
    public class QuizSessionViewModel
    {
        public int QuizId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Subject { get; set; }
        public string Difficulty { get; set; } = "Medium";
        public List<QuizQuestion> Questions { get; set; } = new();
    }
}