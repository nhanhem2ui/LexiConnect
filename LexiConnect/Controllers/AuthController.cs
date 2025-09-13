using BusinessObjects;
using LexiConnect.Models;
using LexiConnect.Models.ViewModels;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Repositories;
using System.Security.Claims;
using System.Text;

namespace LexiConnect.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<Users> _userManager;
        private readonly SignInManager<Users> _signInManager;
        private readonly ISender _emailSender;
        private readonly IGenericRepository<University> _universityRepository;

        public AuthController(UserManager<Users> userManager, SignInManager<Users> signInManager, ISender emailSender, IGenericRepository<University> universityRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _universityRepository = universityRepository;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Signup()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError(string.Empty, "All fields are required.");
                return View("Signup");
            }

            var user = new Users
            {
                UserName = model.Username,
                FullName = model.Email,
                Email = model.Email,
                UniversityId = 0,
                MajorId = 0,
                PointsBalance = 0,
                TotalPointsEarned = 0
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // Assign default role
                await _userManager.AddToRoleAsync(user, "User");

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Action(
                    "ConfirmEmail",
                    "Auth",
                    new { userId = user.Id, code },
                    protocol: Request.Scheme);

                await _emailSender.SendWelcomeEmailAsync(model.Email, model.Username, callbackUrl);

                if (!_userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    TempData["Success"] = "User registered and logged in successfully.";
                    return View("Signin");
                }
                TempData["Success"] = "User registered successfully. Please check your email to confirm your account.";
                return View("Signin");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View("Signup");
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
                return BadRequest("Invalid email confirmation request.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                TempData["Success"] = "Confirm email successfully";
                return RedirectToAction("Introduction", "Home");
            }

            TempData["Error"] = "Error confirming your email.";
            return RedirectToAction("Introduction", "Home");
        }

        [HttpGet]
        public IActionResult Signin()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SigninAsync(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Signin", model);
            }

            var user = await _userManager.FindByNameAsync(model.Username);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View("Signin", model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                // ✅ Clear all existing authentication first
                await _signInManager.SignOutAsync();

                // Ensure University is loaded
                if (user.University == null)
                    user.University = await _universityRepository.GetAsync(u => u.Id == user.UniversityId);

                // Use consistent claim naming and avoid duplicates
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, user.Id),
                    new("FullName", user.FullName),
                    new(ClaimTypes.Email, user.Email),
                    new("UserName", user.UserName),
                    new("AvatarUrl", user.AvatarUrl ?? "~/image/default-avatar.png"),
                    new("UniversityName", user.University?.Name ?? "Unknown"),
                    new(ClaimTypes.Role, "User")
                };

                await _signInManager.SignInWithClaimsAsync(user, model.RememberMe, claims);
                return RedirectToAction("Homepage", "Home");
            }

            if (result.IsLockedOut)
            {
                TempData["Error"] = "User account is locked out.";
                return RedirectToAction("Lockout", "Auth");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View("Signin", model);
        }

        [HttpPost]
        public IActionResult GoogleLogin(string? returnUrl = null)
        {
            var redirectUrl = Url.Action("GoogleCallback", "Auth", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(GoogleDefaults.AuthenticationScheme, redirectUrl);
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public async Task<IActionResult> GoogleCallback(string? returnUrl = null, string? remoteError = null)
        {
            returnUrl ??= Url.Action("Homepage", "Home");

            if (remoteError != null)
            {
                TempData["Error"] = $"Error from external provider: {remoteError}";
                return RedirectToAction("Signin", new { ReturnUrl = returnUrl });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                TempData["Error"] = "Error loading external login information.";
                return RedirectToAction("Signin", new { ReturnUrl = returnUrl });
            }

            var result = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                if (user.University == null)
                    user.University = await _universityRepository.GetAsync(u => u.Id == user.UniversityId);

                // Clear existing authentication completely
                await _signInManager.SignOutAsync();

                // Create clean, consistent claims
                await SignInUserWithClaims(user, false);
                return LocalRedirect(returnUrl);
            }

            if (result.IsLockedOut)
            {
                TempData["Error"] = "User account is locked out.";
                return RedirectToAction("Lockout");
            }

            return await CreateUserFromExternalLogin(info, "/Home/Homepage");
        }

        private async Task<IActionResult> CreateUserFromExternalLogin(ExternalLoginInfo info, string returnUrl)
        {
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var fullName = info.Principal.FindFirstValue(ClaimTypes.Name);
            var avatarUrl = info.Principal.FindFirstValue("picture");

            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Email not received from external provider.";
                return RedirectToAction("Signin");
            }

            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                var addLoginResult = await _userManager.AddLoginAsync(existingUser, info);
                if (addLoginResult.Succeeded)
                {
                    // Update avatar if it's default and we have a better one
                    if (existingUser.AvatarUrl == "~/image/default-avatar.png" && !string.IsNullOrEmpty(avatarUrl))
                    {
                        existingUser.AvatarUrl = avatarUrl;
                        await _userManager.UpdateAsync(existingUser);
                    }

                    // Load university relationship
                    if (existingUser.University == null)
                        existingUser.University = await _universityRepository.GetAsync(u => u.Id == existingUser.UniversityId);

                    await _signInManager.SignOutAsync();
                    await SignInUserWithClaims(existingUser, false);
                    return LocalRedirect(returnUrl);
                }

                foreach (var error in addLoginResult.Errors)
                    TempData["Error"] = error.Description;

                return RedirectToAction("Signin");
            }

            // Create new user
            var user = new Users
            {
                UserName = email,
                Email = email,
                FullName = fullName ?? "External User",
                AvatarUrl = avatarUrl ?? "~/image/default-avatar.png",
                UniversityId = 0,
                MajorId = 0,
                EmailConfirmed = true,
                PointsBalance = 0,
                TotalPointsEarned = 0
            };

            var createResult = await _userManager.CreateAsync(user);
            if (createResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
                var addLoginResult = await _userManager.AddLoginAsync(user, info);

                if (addLoginResult.Succeeded)
                {
                    await _signInManager.SignOutAsync();
                    await SignInUserWithClaims(user, false);
                    return LocalRedirect(returnUrl);
                }
            }

            foreach (var error in createResult.Errors)
                TempData["Error"] = error.Description;

            return RedirectToAction("Signin");
        }

        private async Task SignInUserWithClaims(Users user, bool isPersistent)
        {
            // Ensure University is loaded
            if (user.University == null)
                user.University = await _universityRepository.GetAsync(u => u.Id == user.UniversityId);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new("FullName", user.FullName ?? "DefaultName"),
                new(ClaimTypes.Email, user.Email ?? "EditThisEmail@gmail.com"),
                new("UserName", user.UserName ?? "DefaultUserName"),
                new("AvatarUrl", user.AvatarUrl ?? "~/image/default-avatar.png"),
                new("UniversityName", user.University?.Name ?? "Unknown"),
                new(ClaimTypes.Role, "User")
            };

            await _signInManager.SignInWithClaimsAsync(user, isPersistent, claims);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignoutAsync()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Signin");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email is required.");
            }

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                TempData["Success"] = "If an account with that email exists, we have sent a password reset link.";
                return RedirectToAction("Signin");
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = Url.Action(
                "ResetPassword",
                "Auth",
                new { userId = user.Id, code },
                protocol: Request.Scheme);

            await _emailSender.SendPasswordResetEmailAsync(email, user.UserName, callbackUrl);

            TempData["Success"] = "If an account with that email exists, we have sent a password reset link.";
            return RedirectToAction("Signin");
        }

        [HttpGet]
        public IActionResult ResetPassword(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return BadRequest("Invalid password reset request.");
            }

            var model = new ResetPasswordViewModel
            {
                Code = code,
                UserId = userId
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (string.IsNullOrEmpty(model.UserId) || string.IsNullOrEmpty(model.Code) || string.IsNullOrEmpty(model.NewPassword))
            {
                return BadRequest("All fields are required.");
            }

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return BadRequest("Invalid password reset request.");
            }

            model.Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Code));
            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.NewPassword);

            if (result.Succeeded)
            {
                TempData["Success"] = "Your password has been reset successfully.";
                return RedirectToAction("Signin");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return BadRequest(ModelState);
        }
    }
}