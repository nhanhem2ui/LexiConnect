using BusinessObjects;
using LexiConnect.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories;

namespace LexiConnect.Controllers
{
    public class LibraryController : Controller
    {
        private readonly IGenericRepository<UserFavorite> _userFavoriteRepository;
        private readonly IGenericRepository<UserFollowCourse> _userFollowCourseRepository;
        private readonly IGenericRepository<Document> _documentRepository;
        private readonly IGenericRepository<Course> _courseRepository;
        private readonly UserManager<Users> _userManager;

        public LibraryController(
            IGenericRepository<UserFavorite> userFavoriteRepository,
            IGenericRepository<UserFollowCourse> userFollowCourseRepository,
            IGenericRepository<Document> documentRepository,
            IGenericRepository<Course> courseRepository,
            UserManager<Users> userManager)
        {
            _userFavoriteRepository = userFavoriteRepository;
            _userFollowCourseRepository = userFollowCourseRepository;
            _documentRepository = documentRepository;
            _courseRepository = courseRepository;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // --- Lấy danh sách tài liệu đã favorite ---
            var favoriteDocuments = await _userFavoriteRepository
                .GetAllQueryable(f => f.UserId == currentUser.Id)
                .Include(f => f.Document)
                .ThenInclude(d => d.Course)
                .Select(f => f.Document)
                .ToListAsync();

            // --- Lấy danh sách khoá học đã follow ---
            var followedCourses = await _userFollowCourseRepository
                .GetAllQueryable(fc => fc.UserId == currentUser.Id)
                .Include(fc => fc.Course)
                .Select(fc => fc.Course)
                .ToListAsync();

            var viewModel = new LibraryViewModel
            {
                FavoriteDocuments = favoriteDocuments,
                FollowedCourses = followedCourses
            };

            return View("MyLibrary", viewModel);
        }
    }
}
