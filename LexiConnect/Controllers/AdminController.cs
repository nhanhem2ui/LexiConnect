using BusinessObjects;
using LexiConnect.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Repositories;

namespace LexiConnect.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IGenericRepository<Document> _documentRepository;
        private readonly UserManager<Users> _userManager;
        public AdminController(IGenericRepository<Document> documentRepository, UserManager<Users> userManager)
        {
            _documentRepository = documentRepository;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> AdminManagement()
        {
            var totalDocuments = _documentRepository.GetAllQueryable()
                .Include(d => d.Course)
                .Include(d => d.Uploader);

            var users = _userManager.Users;
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
            var query = _documentRepository.GetAllQueryable()
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
            var allDocuments = await _documentRepository.GetAllQueryable().ToListAsync();

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
            var courses = _documentRepository.GetAllQueryable()
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
            var uploaders = _documentRepository.GetAllQueryable()
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
            var document = await _documentRepository.GetAsync(d => d.DocumentId == id );
            if (document != null)
            {
                document.Status = "approved";
                document.ApprovedAt = DateTime.Now;
                await _documentRepository.UpdateAsync(document);
            }
            return RedirectToAction(nameof(DocumentManagement));
        }

        [HttpPost]
        public async Task<IActionResult> RejectDocument(int id, string rejectionReason = "")
        {
            var document = await _documentRepository.GetAsync(d => d.DocumentId == id);
            if (document != null)
            {
                document.Status = "rejected";
                document.RejectionReason = rejectionReason;
                await _documentRepository.UpdateAsync(document);
            }
            return RedirectToAction(nameof(DocumentManagement));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            try
            {
                await _documentRepository.DeleteAsync(id);
                return Json(new { success = true, message = "Document deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting document: " + ex.Message });
            }
        }
    }
}
