using BusinessObjects;
using LexiConnect.Models;
using LexiConnect.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Repositories;
using System.Diagnostics;
using System.Security.Claims;

namespace LexiConnect.Controllers
{
    public class Home2Controller : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IGenericRepository<University> _universityRepository;
        private readonly IGenericRepository<Users> _userRepository;
        private readonly IGenericRepository<Major> _majorRepository;
        private readonly IGenericRepository<Course> _courseRepository;
        private readonly IGenericRepository<Document> _documentRepository;
        private readonly IGenericRepository<RecentViewed> _recentViewedRepository;
        private readonly IGenericRepository<DocumentLike> _documentLikeRepository;
        private readonly IGenericRepository<UserFollower> _userFollowerRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public Home2Controller(ILogger<HomeController> logger, IGenericRepository<University> universityRepository,
            IGenericRepository<Course> courseRepository, IGenericRepository<Document> documentRepository,
            IGenericRepository<Users> userRepository, IGenericRepository<RecentViewed> recentViewedRepository,
            IGenericRepository<UserFollower> userFollowerRepository, IGenericRepository<Major> majorRepository, IWebHostEnvironment webHostEnvironment, IGenericRepository<DocumentLike> documentLikeRepository)
        {
            _logger = logger;
            _universityRepository = universityRepository;
            _courseRepository = courseRepository;
            _userRepository = userRepository;
            _documentRepository = documentRepository;
            _recentViewedRepository = recentViewedRepository;
            _documentLikeRepository = documentLikeRepository;
            _userFollowerRepository = userFollowerRepository;
            _majorRepository = majorRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public IActionResult Introduction()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Homepage");
            }

            var universities = _universityRepository
                .GetAllQueryable(u => u.IsVerified && u.Id != 0)
                .OrderBy(u => Guid.NewGuid())
                .Take(3);

            var courses = _courseRepository
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
            var userLikedDocs = GetUserLikedDocumentsAsync(topdocuments, userId);

            var model = new HomePageViewModel
            {
                RecentVieweds = recentvieweds,
                TopDocuments = topdocuments,
                UserLikedDocuments = userLikedDocs.Result
            };


            return View(model);
        }



        [HttpGet]
        [Authorize]
        public async Task<IActionResult> UserProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return NotFound("Invalid operant");
            }

            var user = await _userRepository.GetAsync(u => u.Id.Equals(userId));

            if (user != null)
            {
                var uploadedDocuments = _documentRepository.GetAllQueryable(d => d.UploaderId.Equals(userId));
                var upvotes = 0;
                foreach (var document in uploadedDocuments)
                {
                    upvotes += document.LikeCount;
                }
                var follower = _userFollowerRepository.GetAllQueryable(u => u.FollowingId == user.Id);

                var recentActivities = _recentViewedRepository
                    .GetAllQueryable(c => c.UserId.Equals(user.Id))
                    .Include(c => c.Document)
                    .Include(c => c.Course);

                var activeSubscription = true;

                if (user.SubscriptionPlan.PlanId != 1 && user.SubscriptionEndDate > DateTime.Now)
                {
                    activeSubscription = false;
                }

                var model = new UserProfileViewModel
                {
                    User = user,
                    Documents = uploadedDocuments,
                    RecentActivities = recentActivities,
                    FollowerNum = await follower.CountAsync(),
                    ActiveSubscription = activeSubscription,
                    Upvotes = upvotes
                };

                return View(model);
            }
            return NotFound("An error has occured");
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userRepository.GetAsync(u => u.Id.Equals(User.FindFirstValue(ClaimTypes.NameIdentifier)));
            if (user == null)
            {
                return NotFound();
            }

            // Get document count for the user
            var documentCount = await _documentRepository.GetAllQueryable(d => d.UploaderId.Equals(user.Id)).CountAsync();

            // Create view model
            var viewModel = new EditProfileViewModel(user, documentCount);

            // Populate dropdown lists

            var universities = _universityRepository.GetAllQueryable(u => u.Id != 0).ToList();
            ViewBag.Universities = new SelectList(universities, "Id", "Name", user?.UniversityId);

            var majors = _majorRepository
                .GetAllQueryable(m => m.UniversityId == user.UniversityId)
                .ToList();
            ViewBag.Majors = new SelectList(majors, "MajorId", "Name", user?.MajorId);


            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Repopulate dropdown lists if model is invalid
                var universities = _universityRepository.GetAllQueryable(u => u.Id != 0).ToList();
                ViewBag.Universities = new SelectList(universities, "Id", "Name", model?.UniversityId);

                var majors = _majorRepository
                    .GetAllQueryable(m => m.UniversityId == model.UniversityId)
                    .ToList();
                ViewBag.Majors = new SelectList(majors, "MajorId", "Name", model?.MajorId);
                return View(model);
            }

            var user = await _userRepository.GetAsync(u => u.Id.Equals(User.FindFirstValue(ClaimTypes.NameIdentifier)));
            if (user == null)
            {
                return NotFound();
            }

            try
            {
                // Update user properties
                user.FullName = model.FullName;
                user.PhoneNumber = model.PhoneNumber;
                user.UniversityId = model.UniversityId;
                user.MajorId = model.MajorId;

                // Handle avatar upload if provided
                if (model.AvatarFile != null && model.AvatarFile.Length > 0)
                {
                    var avatarUrl = string.Empty;
                    try
                    {

                        // Check file size (5MB limit)
                        if (model.AvatarFile.Length > 5 * 1024 * 1024)
                        {
                            TempData["Error"] = $"{model.AvatarFile}, File size cannot exceed 5MB.";
                            return RedirectToAction("EditProfile");
                        }

                        // Check file type
                        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                        if (!allowedTypes.Contains(model.AvatarFile.ContentType.ToLower()))
                        {
                            TempData["Error"] = $"{model.AvatarFile}, Only JPEG, PNG and GIF images are allowed.";
                            return RedirectToAction("EditProfile");
                        }

                        // Generate unique filename
                        var fileExtension = Path.GetExtension(model.AvatarFile.FileName);
                        var fileName = $"avatar_{user.Id}_{DateTime.Now:yyyyMMdd_HHmmss}{fileExtension}";

                        // Create uploads directory if it doesn't exist
                        var uploadsPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "avatars");
                        if (!Directory.Exists(uploadsPath))
                        {
                            Directory.CreateDirectory(uploadsPath);
                        }

                        // Save file
                        var filePath = Path.Combine(uploadsPath, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.AvatarFile.CopyToAsync(stream);
                        }

                        // Return relative URL
                        avatarUrl = $"~/uploads/avatars/{fileName}";
                    }
                    catch (Exception)
                    {
                        TempData["Error"] = $"{model.AvatarFile}, Error uploading avatar. Please try again.";
                        return RedirectToAction("EditProfile");
                    }

                    if (!string.IsNullOrEmpty(avatarUrl))
                    {
                        // Delete old avatar if it's not the default
                        if (!string.IsNullOrEmpty(user.AvatarUrl) &&
                            user.AvatarUrl != "~/image/default-avatar.png")
                        {
                            try
                            {
                                // Convert relative URL to physical path
                                var relativePath = user.AvatarUrl.Replace("~/", "");
                                var physicalPath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath);

                                if (System.IO.File.Exists(physicalPath))
                                {
                                    System.IO.File.Delete(physicalPath);
                                }
                            }
                            catch (Exception)
                            {
                                TempData["Error"] = "Something wrong, please try again";
                                return RedirectToAction("EditProfile");
                            }
                        }
                        user.AvatarUrl = avatarUrl;
                    }
                }

                // Update user in database
                var result = await _userRepository.UpdateAsync(user);
                if (result)
                {
                    TempData["SuccessMessage"] = "Profile updated successfully!";
                    return RedirectToAction("UserProfile");
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while updating your profile. Please try again.";
            }

            ViewBag.Universities = new SelectList(_universityRepository.GetAllQueryable(u => u.Id != 0)
                .ToList(), "Id", "Name", user?.UniversityId);

            ViewBag.Majors = new SelectList(_majorRepository
                .GetAllQueryable(m => m.UniversityId == user.UniversityId)
                .ToList(), "MajorId", "Name", user?.MajorId);

            return View(model);
        }

        [HttpGet]
        [Authorize]
        public IActionResult GetMajorsByUniversity(int universityId)
        {
            var majors = _majorRepository
                .GetAllQueryable(m => m.UniversityId == universityId)
                .Select(m => new { m.MajorId, m.Name })
                .ToList();
            return Json(majors);
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

            // L?y danh s�ch document IDs m� user ?� like
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
