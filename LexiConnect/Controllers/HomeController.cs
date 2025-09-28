using BusinessObjects;
using LexiConnect.Models;
using LexiConnect.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories;
using System.Diagnostics;
using System.Security.Claims;

namespace LexiConnect.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IGenericRepository<University> _universityRepository;
        private readonly IGenericRepository<Users> _userRepository;
        private readonly IGenericRepository<Course> _courseRepository;
        private readonly IGenericRepository<Document> _documentRepository;
        private readonly IGenericRepository<RecentViewed> _recentViewedRepository;
        private readonly IGenericRepository<DocumentLike> _documentLikeRepository; 
        public HomeController(ILogger<HomeController> logger, IGenericRepository<Users> userRepository, IGenericRepository<DocumentLike> documentLikeRepository, IGenericRepository<University> universityRepository, IGenericRepository<Course> courseRepository, IGenericRepository<Major> majorRepository , IGenericRepository<RecentViewed> recentViewedRepository, IGenericRepository<Document> documentRepository)
        {
            _logger = logger;
            _universityRepository = universityRepository;
            _courseRepository = courseRepository;
            _userRepository = userRepository;
            _documentRepository = documentRepository;
            _recentViewedRepository = recentViewedRepository;
            _documentLikeRepository = documentLikeRepository;

        }

        [HttpGet]
        public IActionResult Introduction()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Homepage");
            }

            var universities = _universityRepository
                .GetAllQueryable(u => u.IsVerified)
                .OrderBy(u => Guid.NewGuid())
                .Take(3);

            var courses = _courseRepository
                .GetAllQueryable(c => c.IsActive)
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
            var recentvieweds = _recentViewedRepository
                .GetAllQueryable()
                .OrderBy(u => Guid.NewGuid())
                .Take(3);

            var topdocuments = _documentRepository
                .GetAllQueryable()
                .Include(c => c.Course)
                .Include(c => c.Uploader)
                .Include(c => c.ApprovedByUser)
                .ThenInclude(m => m.University)
                .OrderByDescending(c => c.LikeCount)
                .Take(3);

            // Load user liked status for top documents
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userLikedDocs =   GetUserLikedDocumentsAsync(topdocuments, userId);

            var model = new HomePageViewModel
            {
                RecentVieweds = recentvieweds ,
                TopDocuments = topdocuments ,
                UserLikedDocuments = userLikedDocs.Result
            };


            return View(model);
        }



        [HttpGet]
        public async Task<IActionResult> UserProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return NotFound("Invalid operant");
            }

            var user = await _userRepository.GetAsync(u => u.Id.Equals(userId));
            var identity = User;

            var userClaims = User.Claims.Select(c => $"{c.Type}: {c.Value}");
            Console.WriteLine(string.Join("\n", userClaims));

            if (user != null)
            {
                var uploadedDocuments = _documentRepository.GetAllQueryable(d => d.UploaderId.Equals(userId)).ToList();
                var upvotes = 0;
                foreach (var document in uploadedDocuments)
                {
                    upvotes += document.LikeCount;
                }
                var model = new UserProfileViewModel
                {
                    User = user,
                    UploadedNum = uploadedDocuments.Count,
                    Upvotes = upvotes
                };
                return View(model);
            }
            return NotFound("An error has occured");
        }
        [HttpGet]
        public IActionResult EditProfile()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

      

        // Method ?? load like status cho user
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

            // L?y danh sách document IDs mà user ?ã like
            var likedDocumentIds = await _documentLikeRepository
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

    }
}
