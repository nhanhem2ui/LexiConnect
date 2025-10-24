using BusinessObjects;
using LexiConnect.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories;

namespace LexiConnect.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IGenericRepository<Users> _userRepository;
        private readonly IGenericRepository<Document> _documentRepository;
        private readonly IGenericRepository<UserFollower> _followerRepository;
        private readonly UserManager<Users> _userManager;
        public ProfileController(
            IGenericRepository<Users> userRepository,
            IGenericRepository<Document> documentRepository,
            IGenericRepository<UserFollower> followerRepository,
            UserManager<Users> userManager)
        {
            _userRepository = userRepository;
            _documentRepository = documentRepository;
            _followerRepository = followerRepository;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> PublicUserProfile(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            // Get the target user with related data
            var user = await _userRepository.GetAllQueryable(u => u.Id == id)
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
            var documents = await _documentRepository.GetAllQueryable(d =>
                d.UploaderId == id && d.Status == "approved")
                .Include(d => d.Course)
                .OrderByDescending(d => d.ViewCount)
                .Take(10)
                .ToListAsync();

            // Get follower/following counts
            var followerCount = await _followerRepository.GetAllQueryable(f => f.FollowingId == id)
                .CountAsync();
            var followingCount = await _followerRepository.GetAllQueryable(f => f.FollowerId == id)
                .CountAsync();

            // Check if current user is following this user
            var isFollowing = false;
            if (currentUser != null && !isOwnProfile)
            {
                isFollowing = await _followerRepository.ExistsAsync(f =>
                    f.FollowerId == currentUser.Id && f.FollowingId == id);
            }

            // Calculate statistics
            var totalUploads = documents.Count;
            var totalUpvotes = documents.Sum(d => d.LikeCount);
            var totalComments = 0; // You'll need to implement comments repository
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
                return RedirectToAction("PublicProfile", new { id = userId });
            }

            // Check if already following
            var existingFollow = await _followerRepository.GetAllQueryable(f =>
                f.FollowerId == currentUser.Id && f.FollowingId == userId)
                .FirstOrDefaultAsync();

            if (existingFollow != null)
            {
                // Unfollow
                await _followerRepository.DeleteAsync(existingFollow.Id);
                TempData["Success"] = "You have unfollowed this user.";
            }
            else
            {
                // Follow
                var newFollow = new UserFollower
                {
                    FollowerId = currentUser.Id,
                    FollowingId = userId,
                    CreatedAt = DateTime.Now
                };
                await _followerRepository.AddAsync(newFollow);
                TempData["Success"] = "You are now following this user.";
            }

            return RedirectToAction("PublicProfile", new { id = userId });
        }

        [HttpGet]
        public async Task<IActionResult> Followers(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var followers = await _followerRepository.GetAllQueryable(f => f.FollowingId == id)
                .Include(f => f.Follower)
                    .ThenInclude(u => u.University)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => f.Follower)
                .ToListAsync();

            ViewBag.UserId = id;
            ViewBag.ListType = "Followers";
            return View("FollowList", followers);
        }

        [HttpGet]
        public async Task<IActionResult> Following(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var following = await _followerRepository.GetAllQueryable(f => f.FollowerId == id)
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
