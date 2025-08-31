using BusinessObjects;
using LexiConnect.Models;
using LexiConnect.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Repositories;
using System.Diagnostics;
using System.Security.Claims;

namespace LexiConnect.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<Users> _userManager;
        private readonly IGenericRepository<University> _universityRepository;
        public HomeController(ILogger<HomeController> logger, UserManager<Users> userManager, IGenericRepository<University> universityRepository)
        {
            _logger = logger;
            _userManager = userManager;
            _universityRepository = universityRepository;
        }

        [HttpGet]
        public IActionResult Introduction()
        {
            var universities = _universityRepository.GetAllQueryable(u => u.IsVerified);

            var randomUni = universities
                .OrderBy(u => Guid.NewGuid())
                .Take(3);

            var model = new IntroductionViewModel
            {
                Universities = randomUni,

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
