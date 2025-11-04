using BusinessObjects;
using LexiConnect.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Services;
using System.Security.Claims;

namespace LexiConnect.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IGenericService<Users> _userService;
        private readonly IGenericService<Document> _documentService;
        private readonly IGenericService<UserFollower> _followerService;
        private readonly UserManager<Users> _userManager;
        private readonly IGenericService<University> _universityService;
        private readonly IGenericService<Major> _majorService;
        private readonly IGenericService<RecentViewed> _recentViewedService;
        private readonly IGenericService<DocumentReview> _documentReviewService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProfileController(
            IGenericService<Users> userService, IGenericService<Document> documentService,
            IGenericService<UserFollower> followerService, IGenericService<RecentViewed> recentViewedService,
            UserManager<Users> userManager, IWebHostEnvironment webHostEnvironment,
            IGenericService<University> universityService, IGenericService<Major> majorService, IGenericService<DocumentReview> documentReviewService)
        {
            _userService = userService;
            _documentService = documentService;
            _followerService = followerService;
            _userManager = userManager;
            _recentViewedService = recentViewedService;
            _webHostEnvironment = webHostEnvironment;
            _universityService = universityService;
            _majorService = majorService;
            _documentReviewService = documentReviewService;
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

            var user = await _userService.GetAsync(u => u.Id.Equals(userId));

            if (user != null)
            {
                var uploadedDocuments = _documentService.GetAllQueryable(d => d.UploaderId.Equals(userId));
                var upvotes = 0;
                foreach (var document in uploadedDocuments)
                {
                    upvotes += document.LikeCount;
                }
                var follower = _followerService.GetAllQueryable(u => u.FollowingId == user.Id);

                var recentActivities = _recentViewedService
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
            var user = await _userService.GetAsync(u => u.Id.Equals(User.FindFirstValue(ClaimTypes.NameIdentifier)));
            if (user == null)
            {
                return NotFound();
            }

            // Get document count for the user
            var documentCount = await _documentService.GetAllQueryable(d => d.UploaderId.Equals(user.Id)).CountAsync();

            // Create view model
            var viewModel = new EditProfileViewModel(user, documentCount);

            // Populate dropdown lists

            var universities = _universityService.GetAllQueryable(u => u.Id != 0).ToList();
            ViewBag.Universities = new SelectList(universities, "Id", "Name", user?.UniversityId);

            var majors = _majorService
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
                var universities = _universityService.GetAllQueryable(u => u.Id != 0).ToList();
                ViewBag.Universities = new SelectList(universities, "Id", "Name", model?.UniversityId);

                var majors = _majorService
                    .GetAllQueryable(m => m.UniversityId == model.UniversityId)
                    .ToList();
                ViewBag.Majors = new SelectList(majors, "MajorId", "Name", model?.MajorId);
                return View(model);
            }

            var user = await _userService.GetAsync(u => u.Id.Equals(User.FindFirstValue(ClaimTypes.NameIdentifier)));
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
                var result = await _userService.UpdateAsync(user);
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

            ViewBag.Universities = new SelectList(_universityService.GetAllQueryable(u => u.Id != 0)
                .ToList(), "Id", "Name", user?.UniversityId);

            ViewBag.Majors = new SelectList(_majorService
                .GetAllQueryable(m => m.UniversityId == user.UniversityId)
                .ToList(), "MajorId", "Name", user?.MajorId);

            return View(model);
        }

        [HttpGet]
        [Authorize]
        public IActionResult GetMajorsByUniversity(int universityId)
        {
            var majors = _majorService
                .GetAllQueryable(m => m.UniversityId == universityId)
                .Select(m => new { m.MajorId, m.Name })
                .ToList();
            return Json(majors);
        }

        [HttpGet]
        public async Task<IActionResult> PublicUserProfile(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            // Get the target user with related data
            var user = await _userService.GetAllQueryable(u => u.Id == id)
                .Include(u => u.University)
                .Include(u => u.Major)
                .Include(u => u.SubscriptionPlan)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            // Get current logged-in user
            var currentUser = await _userManager.GetUserAsync(User);
            var isOwnProfile = currentUser?.Id == id;

            // Get user's documents
            var documents = await _documentService.GetAllQueryable(d =>
                d.UploaderId == id && d.Status == "approved")
                .Include(d => d.Course)
                .OrderByDescending(d => d.ViewCount)
                .Take(10)
                .ToListAsync();

            // Get follower/following counts
            var followerCount = await _followerService.GetAllQueryable(f => f.FollowingId.Equals(id))
                .CountAsync();
            var followingCount = await _followerService.GetAllQueryable(f => f.FollowerId.Equals(id))
                .CountAsync();

            // Check if current user is following this user
            var isFollowing = false;
            if (currentUser != null && !isOwnProfile)
            {
                isFollowing = await _followerService.ExistsAsync(f =>
                    f.FollowerId == currentUser.Id && f.FollowingId == id);
            }

            // Calculate statistics
            var totalUploads = documents.Count;
            var totalUpvotes = documents.Sum(d => d.LikeCount);
            var totalComments = await _documentReviewService.GetAllQueryable(d => d.ReviewerId.Equals(id) && d.Comment != string.Empty && d.Comment != null).CountAsync();
            var studentsHelped = documents.Sum(d => d.ViewCount);

            // Prepare popular documents with ratings
            var popularDocuments = documents.Select(d => new PublicUserProfileViewModel.DocumentWithStats
            {
                Document = d,
                Rating = d.LikeCount > 0 ? (int)((double)d.LikeCount / (d.LikeCount + d.DownloadCount) * 100) : 100,
                TotalRatings = d.LikeCount + d.DownloadCount
            }).ToList();

            // Build view model
            var viewModel = new PublicUserProfileViewModel
            {
                User = user,
                TotalUploads = totalUploads,
                TotalUpvotes = totalUpvotes,
                TotalComments = totalComments,
                StudentsHelped = studentsHelped,
                FollowerCount = followerCount,
                FollowingCount = followingCount,
                IsFollowing = isFollowing,
                IsOwnProfile = isOwnProfile,
                PopularDocuments = popularDocuments,
                RecentActivities = new List<PublicUserProfileViewModel.RecentActivity>()
            };

            return View(viewModel);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFollow(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (currentUser.Id == userId)
            {
                TempData["Error"] = "You cannot follow yourself.";
                return RedirectToAction("PublicUserProfile", new { id = userId });
            }

            // Check if already following
            var existingFollow = await _followerService.GetAllQueryable(f =>
                f.FollowerId == currentUser.Id && f.FollowingId == userId)
                .FirstOrDefaultAsync();

            if (existingFollow != null)
            {
                // Unfollow
                await _followerService.DeleteAsync(existingFollow.Id);
                TempData["Success"] = "You have unfollowed this user.";
            }
            else
            {
                // Follow
                var newFollow = new UserFollower
                {
                    FollowerId = currentUser.Id,
                    FollowingId = userId,
                    CreatedAt = DateTime.Now,
                };
                await _followerService.AddAsync(newFollow);
                TempData["Success"] = "You are now following this user.";
            }

            return RedirectToAction("PublicUserProfile", new { id = userId });
        }

        [HttpGet]
        public async Task<IActionResult> Followers(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var followers = await _followerService.GetAllQueryable(f => f.FollowingId == id)
                .Include(f => f.Follower)
                    .ThenInclude(u => u.University)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => f.Follower)
                .ToListAsync();

            ViewBag.UserId = id;
            ViewBag.ListType = "Followers";
            return View("FollowList", followers);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Following(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var following = await _followerService.GetAllQueryable(f => f.FollowerId == id)
                .Include(f => f.Following)
                    .ThenInclude(u => u.University)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => f.Following)
                .ToListAsync();

            ViewBag.UserId = id;
            ViewBag.ListType = "Following";
            return View("FollowList", following);
        }
    }
}
