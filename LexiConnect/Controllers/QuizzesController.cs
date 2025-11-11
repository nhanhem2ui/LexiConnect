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

        public QuizzesController(IGenericService<Quiz> quizService,IGenericService<QuizQuestion> quizQuestionService)
        {
            _quizService = quizService;
            _quizQuestionService = quizQuestionService;
        }

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

        // GET: /Quiz/Take/5
        // This is just a short session quiz (not persisted)
        public async Task<IActionResult> TakeQuiz(int id)
        {
            var quiz = await _quizService.GetAsync(q => q.QuizId == id);
            var questions = _quizQuestionService.GetAllQueryable(q => q.QuizId == quiz.QuizId);
            if (quiz == null)
                return NotFound();

            // You could pass only questions and title to the view for simplicity
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
    }

    // Simple ViewModel for the in-memory quiz
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
