using Microsoft.AspNetCore.Mvc;

namespace LexiConnect.Controllers
{
    public class AuthController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Signup()
        {
            return View();
        }
    }
}
