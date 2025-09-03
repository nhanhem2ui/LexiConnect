using BusinessObjects;
using LexiConnect.Models;
using LexiConnect.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LexiConnect.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<Users> _userManager;
        private readonly IGenericRepository<University> _universityRepository;
        private readonly IGenericRepository<Course> _courseRepository;
        private readonly IGenericRepository<Major> _majorRepository;
        public HomeController(ILogger<HomeController> logger, UserManager<Users> userManager, IGenericRepository<University> universityRepository, IGenericRepository<Course> courseRepository, IGenericRepository<Major> majorRepository)
        {
            _logger = logger;
            _userManager = userManager;
            _universityRepository = universityRepository;
            _courseRepository = courseRepository;
            _majorRepository = majorRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Introduction()
        {
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
        public async Task<IActionResult> UserProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return NotFound("Invalid operant");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var model = new UserProfileViewModel
                {
                    User = user,
                };
                return View();
            }
            return NotFound("An error has occured");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
