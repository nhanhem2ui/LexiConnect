using BusinessObjects;
using LexiConnect.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Services;

namespace LexiConnect.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IGenericService<Document> _documentService;
        private readonly IGenericService<Users> _usersService;
        private readonly UserManager<Users> _userManager;
        public AdminController(IGenericService<Document> documentService, IGenericService<Users> userService, UserManager<Users> userManager)
        {
            _documentService = documentService;
            _usersService = userService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> AdminManagement()
        {
            var totalDocuments = _documentService.GetAllQueryable()
                .Include(d => d.Course)
                .Include(d => d.Uploader);

            var users = _usersService.GetAllQueryable().Include(u => u.SubscriptionPlan);
            var pendingDocuments = totalDocuments.Where(d => d.Status == "pending" || d.Status == "processing");
            var flaggedContent = totalDocuments.Where(d => d.Status == "flagged");

            var model = new AdminManagementViewModel
            {
                RecentUsers = users,
                ActiveSubscriptions = await users.Where(s => s.SubscriptionPlan.Name != "FREE" && s.SubscriptionStartDate.HasValue && s.SubscriptionEndDate.HasValue).CountAsync(),
                TotalUsers = await users.CountAsync(),
                PendingDocuments = await pendingDocuments.CountAsync(),
                FlaggedDocuments = await flaggedContent.CountAsync(),
                FlaggedContent = flaggedContent,
                RecentPendingDocuments = pendingDocuments,
                TotalDocuments = await totalDocuments.CountAsync(),
                TodaysUploads = await totalDocuments.Where(d => d.CreatedAt == DateTime.Today).CountAsync(),
            };

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> DocumentManagement(
            string search = "",
            string status = "",
            string type = "",
            string course = "",
            string uploader = "",
            string sortBy = "CreatedAt",
            string sortOrder = "desc",
            int page = 1,
            int pageSize = 10)
        {
            // Store filter values in ViewBag for form persistence
            ViewBag.SearchTerm = search;
            ViewBag.StatusFilter = status;
            ViewBag.TypeFilter = type;
            ViewBag.CourseFilter = course;
            ViewBag.UploaderFilter = uploader;
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;

            // Start with base query including related entities
            var query = _documentService.GetAllQueryable()
                .Include(d => d.Uploader)
                    .ThenInclude(u => u.University)
                .Include(d => d.Course)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(d =>
                    d.Title.ToLower().Contains(searchLower) ||
                    d.Description.ToLower().Contains(searchLower) ||
                    d.Uploader.FullName.ToLower().Contains(searchLower) ||
                    d.Uploader.UserName.ToLower().Contains(searchLower) ||
                    (d.Uploader.FullName + " " + d.Uploader.UserName).ToLower().Contains(searchLower) ||
                    d.Course.CourseName.ToLower().Contains(searchLower) ||
                    d.Course.CourseCode.ToLower().Contains(searchLower)
                );
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(d => d.Status.ToLower() == status.ToLower());
            }

            // Apply type filter
            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(d => d.DocumentType.ToLower() == type.ToLower());
            }

            // Apply course filter
            if (!string.IsNullOrEmpty(course))
            {
                query = query.Where(d => d.CourseId.ToString() == course);
            }

            // Apply uploader filter
            if (!string.IsNullOrEmpty(uploader))
            {
                query = query.Where(d => d.UploaderId == uploader);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "title" => sortOrder.ToLower() == "asc"
                    ? query.OrderBy(d => d.Title)
                    : query.OrderByDescending(d => d.Title),
                "viewcount" => sortOrder.ToLower() == "asc"
                    ? query.OrderBy(d => d.ViewCount)
                    : query.OrderByDescending(d => d.ViewCount),
                "downloadcount" => sortOrder.ToLower() == "asc"
                    ? query.OrderBy(d => d.DownloadCount)
                    : query.OrderByDescending(d => d.DownloadCount),
                "likecount" => sortOrder.ToLower() == "asc"
                    ? query.OrderBy(d => d.LikeCount)
                    : query.OrderByDescending(d => d.LikeCount),
                "status" => sortOrder.ToLower() == "asc"
                    ? query.OrderBy(d => d.Status)
                    : query.OrderByDescending(d => d.Status),
                "uploader" => sortOrder.ToLower() == "asc"
                    ? query.OrderBy(d => d.Uploader.FullName).ThenBy(d => d.Uploader.UserName)
                    : query.OrderByDescending(d => d.Uploader.FullName).ThenByDescending(d => d.Uploader.UserName),
                _ => sortOrder.ToLower() == "asc"
                    ? query.OrderBy(d => d.CreatedAt)
                    : query.OrderByDescending(d => d.CreatedAt)
            };

            // Apply pagination
            var pagedDocuments = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Calculate pagination info
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            // Calculate statistics
            var allDocuments = await _documentService.GetAllQueryable().ToListAsync();

            var model = new DocumentManagementViewModel
            {
                Documents = pagedDocuments,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize,
                TotalCount = totalCount,
                SearchTerm = search,
                StatusFilter = status,
                TypeFilter = type,
                CourseFilter = course,
                UploaderFilter = uploader,
                SortBy = sortBy,
                SortOrder = sortOrder,
                // Statistics
                TotalDocuments = allDocuments.Count,
                PendingDocuments = allDocuments.Count(d => d.Status == "pending"),
                FlaggedDocuments = allDocuments.Count(d => d.Status == "flagged"),
            };

            // Setup filter dropdown lists
            SetupFilterDropdowns(course, uploader, status, type, sortBy, sortOrder);

            return View(model);
        }

        private void SetupFilterDropdowns(string courseFilter, string uploaderFilter, string statusFilter, string typeFilter, string sortBy, string sortOrder)
        {
            // Status filter dropdown
            ViewBag.StatusFilterList = new SelectList(new[]
            {
                new { Value = "", Text = "All Status" },
                new { Value = "pending", Text = "Pending" },
                new { Value = "approved", Text = "Approved" },
                new { Value = "rejected", Text = "Rejected" },
                new { Value = "flagged", Text = "Flagged" },
                new { Value = "processing", Text = "Processing" }
            }, "Value", "Text", statusFilter);

            // Type filter dropdown
            ViewBag.TypeFilterList = new SelectList(new[]
            {
                new { Value = "", Text = "All Types" },
                new { Value = "notes", Text = "Notes" },
                new { Value = "quiz", Text = "Quiz" },
                new { Value = "assignment", Text = "Assignment" },
                new { Value = "exam", Text = "Exam" },
                new { Value = "study_guide", Text = "Study Guide" },
                new { Value = "flashcards", Text = "Flashcards" },
                new { Value = "presentation", Text = "Presentation" },
                new { Value = "textbook", Text = "Textbook" },
                new { Value = "other", Text = "Other" }
            }, "Value", "Text", typeFilter);

            // Sort by dropdown
            ViewBag.SortByList = new SelectList(new[]
            {
                new { Value = "CreatedAt", Text = "Date Created" },
                new { Value = "Title", Text = "Title" },
                new { Value = "ViewCount", Text = "View Count" },
                new { Value = "DownloadCount", Text = "Downloads" },
                new { Value = "LikeCount", Text = "Likes" },
                new { Value = "Status", Text = "Status" },
                new { Value = "Uploader", Text = "Uploader" }
            }, "Value", "Text", sortBy);

            // Sort order dropdown
            ViewBag.SortOrderList = new SelectList(new[]
            {
                new { Value = "desc", Text = "Descending" },
                new { Value = "asc", Text = "Ascending" }
            }, "Value", "Text", sortOrder);

            // Course filter dropdown (populated dynamically)
            var courseOptions = new List<SelectListItem> { new SelectListItem { Value = "", Text = "All Courses" } };
            var courses = _documentService.GetAllQueryable()
                .Include(d => d.Course)
                .Select(d => d.Course)
                .Distinct()
                .OrderBy(c => c.CourseName);

            foreach (var course in courses)
            {
                courseOptions.Add(new SelectListItem
                {
                    Value = course.CourseId.ToString(),
                    Text = $"{course.CourseCode} - {course.CourseName}",
                    Selected = course.CourseId.ToString() == courseFilter
                });
            }
            ViewBag.CourseFilterList = courseOptions;

            // Uploader filter dropdown (populated dynamically)
            var uploaderOptions = new List<SelectListItem> { new SelectListItem { Value = "", Text = "All Uploaders" } };
            var uploaders = _documentService.GetAllQueryable()
                .Include(d => d.Uploader)
                .Select(d => d.Uploader)
                .Distinct()
                .OrderBy(u => u.FullName)
                .ThenBy(u => u.UserName);

            foreach (var user in uploaders)
            {
                uploaderOptions.Add(new SelectListItem
                {
                    Value = user.Id,
                    Text = $"{user.FullName}",
                    Selected = user.Id == uploaderFilter
                });
            }
            ViewBag.UploaderFilterList = uploaderOptions;
        }

        // Document action methods
        [HttpPost]
        public async Task<IActionResult> ApproveDocument(int id)
        {
            var document = await _documentService.GetAsync(d => d.DocumentId == id);
            if (document != null)
            {
                document.Status = "approved";
                document.ApprovedAt = DateTime.Now;
                await _documentService.UpdateAsync(document);
            }
            return RedirectToAction(nameof(DocumentManagement));
        }

        [HttpPost]
        public async Task<IActionResult> RejectDocument(int id, string rejectionReason = "")
        {
            var document = await _documentService.GetAsync(d => d.DocumentId == id);
            if (document != null)
            {
                document.Status = "rejected";
                document.RejectionReason = rejectionReason;
                await _documentService.UpdateAsync(document);
            }
            return RedirectToAction(nameof(DocumentManagement));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            try
            {
                await _documentService.DeleteAsync(id);
                return Json(new { success = true, message = "Document deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting document: " + ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> UserManagement(
            string search = "",
            string role = "",
            string subscription = "",
            string university = "",
            string sortBy = "CreatedDate",
            string sortOrder = "desc",
            int page = 1,
            int pageSize = 10)
        {
            ViewBag.SearchTerm = search;
            ViewBag.RoleFilter = role;
            ViewBag.SubscriptionFilter = subscription;
            ViewBag.UniversityFilter = university;
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;

            var query = _usersService.GetAllQueryable()
                .Include(u => u.University)
                .Include(u => u.Major)
                .Include(u => u.SubscriptionPlan)
                .Include(u => u.UploadedDocuments)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(u =>
                    u.FullName.ToLower().Contains(searchLower) ||
                    u.UserName.ToLower().Contains(searchLower) ||
                    u.Email.ToLower().Contains(searchLower) ||
                    (u.University != null && u.University.Name.ToLower().Contains(searchLower))
                );
            }

            // Apply subscription filter
            if (!string.IsNullOrEmpty(subscription))
            {
                if (subscription == "free")
                {
                    query = query.Where(u => u.SubscriptionPlan == null || u.SubscriptionPlan.Name == "FREE");
                }
                else if (subscription == "active")
                {
                    query = query.Where(u => u.SubscriptionPlan != null &&
                                           u.SubscriptionPlan.Name != "FREE" &&
                                           u.SubscriptionStartDate.HasValue &&
                                           u.SubscriptionEndDate.HasValue &&
                                           u.SubscriptionEndDate.Value > DateTime.Now);
                }
                else if (subscription == "expired")
                {
                    query = query.Where(u => u.SubscriptionEndDate.HasValue &&
                                           u.SubscriptionEndDate.Value <= DateTime.Now);
                }
            }

            // Apply university filter
            if (!string.IsNullOrEmpty(university))
            {
                query = query.Where(u => u.UniversityId.ToString() == university);
            }

            var totalCount = await query.CountAsync();

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "fullname" => sortOrder.ToLower() == "asc"
                    ? query.OrderBy(u => u.FullName)
                    : query.OrderByDescending(u => u.FullName),
                "email" => sortOrder.ToLower() == "asc"
                    ? query.OrderBy(u => u.Email)
                    : query.OrderByDescending(u => u.Email),
                "points" => sortOrder.ToLower() == "asc"
                    ? query.OrderBy(u => u.PointsBalance)
                    : query.OrderByDescending(u => u.PointsBalance),
                "uploads" => sortOrder.ToLower() == "asc"
                    ? query.OrderBy(u => u.UploadedDocuments.Count)
                    : query.OrderByDescending(u => u.UploadedDocuments.Count),
                "subscription" => sortOrder.ToLower() == "asc"
                    ? query.OrderBy(u => u.SubscriptionPlan.Name)
                    : query.OrderByDescending(u => u.SubscriptionPlan.Name),
                _ => sortOrder.ToLower() == "asc"
                    ? query.OrderBy(u => u.Id)
                    : query.OrderByDescending(u => u.Id)
            };

            var pagedUsers = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            var allUsers = await _usersService.GetAllQueryable().ToListAsync();

            var model = new UserManagementViewModel
            {
                Users = pagedUsers,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize,
                TotalCount = totalCount,
                SearchTerm = search,
                RoleFilter = role,
                SubscriptionFilter = subscription,
                UniversityFilter = university,
                SortBy = sortBy,
                SortOrder = sortOrder,
                TotalUsers = allUsers.Count,
                ActiveSubscriptions = allUsers.Count(u => u.SubscriptionPlan != null &&
                                                        u.SubscriptionPlan.Name != "FREE" &&
                                                        u.SubscriptionEndDate.HasValue &&
                                                        u.SubscriptionEndDate > DateTime.Now),
                NewUsersThisMonth = allUsers.Count(u => u.EmailConfirmed)
            };

            SetupUserFilterDropdowns(subscription, university, sortBy, sortOrder);

            return View(model);
        }

        private void SetupUserFilterDropdowns(string subscriptionFilter, string universityFilter, string sortBy, string sortOrder)
        {
            ViewBag.SubscriptionFilterList = new SelectList(new[]
            {
                new { Value = "", Text = "All Subscriptions" },
                new { Value = "free", Text = "Free" },
                new { Value = "active", Text = "Active Premium" },
                new { Value = "expired", Text = "Expired" }
            }, "Value", "Text", subscriptionFilter);

            ViewBag.SortByList = new SelectList(new[]
            {
                new { Value = "CreatedDate", Text = "Registration Date" },
                new { Value = "FullName", Text = "Name" },
                new { Value = "Email", Text = "Email" },
                new { Value = "Points", Text = "Points Balance" },
                new { Value = "Uploads", Text = "Uploads Count" },
                new { Value = "Subscription", Text = "Subscription" }
            }, "Value", "Text", sortBy);

            ViewBag.SortOrderList = new SelectList(new[]
            {
                new { Value = "desc", Text = "Descending" },
                new { Value = "asc", Text = "Ascending" }
            }, "Value", "Text", sortOrder);

            var universityOptions = new List<SelectListItem> { new SelectListItem { Value = "", Text = "All Universities" } };
            var universities = _usersService.GetAllQueryable()
                .Include(u => u.University)
                .Where(u => u.University != null)
                .Select(u => u.University)
                .Distinct()
                .OrderBy(uni => uni.Name);

            foreach (var uni in universities)
            {
                universityOptions.Add(new SelectListItem
                {
                    Value = uni.Id.ToString(),
                    Text = uni.Name,
                    Selected = uni.Id.ToString() == universityFilter
                });
            }
            ViewBag.UniversityFilterList = universityOptions;
        }

        [HttpPost]
        public async Task<IActionResult> SuspendUser(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user != null)
                {
                    user.LockoutEnabled = true;
                    user.LockoutEnd = DateTimeOffset.MaxValue;
                    await _userManager.UpdateAsync(user);
                    return Json(new { success = true, message = "User suspended successfully." });
                }
                return Json(new { success = false, message = "User not found." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UnsuspendUser(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user != null)
                {
                    user.LockoutEnd = null;
                    await _userManager.UpdateAsync(user);
                    return Json(new { success = true, message = "User unsuspended successfully." });
                }
                return Json(new { success = false, message = "User not found." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user != null)
                {
                    var result = await _userManager.DeleteAsync(user);
                    if (result.Succeeded)
                    {
                        return Json(new { success = true, message = "User deleted successfully." });
                    }
                    return Json(new { success = false, message = "Failed to delete user." });
                }
                return Json(new { success = false, message = "User not found." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AdjustUserPoints(string id, int pointsChange, string reason)
        {
            try
            {
                var user = await _usersService.GetAsync(u => u.Id == id);
                if (user != null)
                {
                    user.PointsBalance += pointsChange;
                    if (pointsChange > 0)
                    {
                        user.TotalPointsEarned += pointsChange;
                    }
                    await _usersService.UpdateAsync(user);
                    return Json(new { success = true, message = "Points adjusted successfully." });
                }
                return Json(new { success = false, message = "User not found." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
    }
}
