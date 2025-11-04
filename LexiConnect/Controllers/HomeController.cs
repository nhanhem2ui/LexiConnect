using BusinessObjects;
using LexiConnect.Models;
using LexiConnect.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Services;
using System.Diagnostics;
using System.Security.Claims;

namespace LexiConnect.Controllers
{
    public class HomeController : Controller
    {
        private readonly IGenericService<University> _universityService;
        private readonly IGenericService<Major> _majorService;
        private readonly IGenericService<Course> _courseService;
        private readonly IGenericService<Document> _documentService;
        private readonly IGenericService<RecentViewed> _recentViewedService;
        private readonly IGenericService<DocumentLike> _documentLikeService;

        public HomeController(IGenericService<University> universityService,
            IGenericService<Course> courseService, IGenericService<Document> documentService,
            IGenericService<RecentViewed> recentViewedService,
            IGenericService<Major> majorService, IGenericService<DocumentLike> documentLikeService)
        {
            _universityService = universityService;
            _courseService = courseService;
            _documentService = documentService;
            _recentViewedService = recentViewedService;
            _documentLikeService = documentLikeService;
            _majorService = majorService;
        }

        [HttpGet]
        public IActionResult Introduction()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Homepage");
            }

            var universities = _universityService
                .GetAllQueryable(u => u.IsVerified && u.Id != 0)
                .OrderBy(u => Guid.NewGuid())
                .Take(3);

            var courses = _courseService
                .GetAllQueryable(c => c.IsActive && c.CourseId != 0)
                .Include(c => c.Major)
                .ThenInclude(m => m.University)
                .OrderBy(u => Guid.NewGuid())
                .Take(3);

            var model = new IntroductionViewModel
            {
                Universities = universities,
                Courses = courses
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult Homepage()
        {
            var recentvieweds = _recentViewedService
                .GetAllQueryable()
                .OrderBy(u => Guid.NewGuid())
                .Take(3);

            var topdocuments = _documentService
                .GetAllQueryable()
                .Include(c => c.Course)
                .Include(c => c.Uploader)
                .Include(c => c.ApprovedByUser)
                .ThenInclude(m => m.University)

                .OrderByDescending(c => c.LikeCount)
                .Take(3);

            // Load user liked status for top documents
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userLikedDocs = GetUserLikedDocumentsAsync(topdocuments, userId);

            var model = new HomePageViewModel
            {
                RecentVieweds = recentvieweds,
                TopDocuments = topdocuments,
                UserLikedDocuments = userLikedDocs.Result
            };


            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task<Dictionary<int, bool>> GetUserLikedDocumentsAsync(IQueryable<Document> documents, string userId)
        {
            var userLikedDocuments = new Dictionary<int, bool>();

            if (string.IsNullOrEmpty(userId))
            {
                // N?u user ch?a ??ng nh?p, t?t c? documents ??u ch?a ???c like
                foreach (var doc in documents)
                {
                    userLikedDocuments[doc.DocumentId] = false;
                }
                return userLikedDocuments;
            }

            var likedDocumentIds = await _documentLikeService
                .GetAllQueryable(dl => dl.UserId == userId, asNoTracking: true)
                .Select(dl => dl.DocumentId)
                .ToListAsync();

            // Populate dictionary
            foreach (var doc in documents)
            {
                userLikedDocuments[doc.DocumentId] = likedDocumentIds.Contains(doc.DocumentId);
            }

            return userLikedDocuments;
        }

        [HttpGet]
        public IActionResult NotFoundPage()
        {
            return View();
        }
    }
}
