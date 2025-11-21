using BusinessObjects;
using LexiConnect.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Services;

namespace LexiConnect.Controllers
{
    public class DocumentController : Controller
    {
        private readonly IGenericService<Document> _documentService;
        private readonly IGenericService<Users> _userService;
        private readonly UserManager<Users> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly IGenericService<DocumentLike> _documentLikeService;
        private readonly IGenericService<UserFavorite> _userFavoriteService;
        private readonly IGenericService<RecentViewed> _recentViewedService;


        public DocumentController(
            IGenericService<Document> documentService,
            IGenericService<Users> userService,
            IGenericService<DocumentLike> documentLikeService,
             IGenericService<UserFavorite> userFavoriteService,
             IGenericService<RecentViewed> recentViewedService,
            UserManager<Users> userManager,
            IWebHostEnvironment environment)
        {
            _documentService = documentService;
            _userService = userService;
            _userManager = userManager;
            _environment = environment;
            _documentService = documentService;
            _documentLikeService = documentLikeService;
            _userFavoriteService = userFavoriteService;
            _recentViewedService = recentViewedService;
        }



        // GET: Document/Details/{id}
        [HttpGet]
        public async Task<IActionResult> DetailDocument(int id)
        {
            try
            {
                // Get document with eager loading for related entities
                var document = await _documentService.GetAllQueryable(d => d.DocumentId == id)
                    .Include(d => d.Course)
                    .Include(d => d.Uploader)
                    .Include(d => d.Uploader.University)
                    .FirstOrDefaultAsync();

                if (document == null)
                {
                    TempData["Error"] = "Document not found.";
                    return RedirectToAction("Homepage", "Home");
                }

                // Determine which file to use for viewing (PDF version if available)
                string viewFilePath = GetViewFilePath(document);
                if (string.IsNullOrEmpty(viewFilePath))
                {
                    TempData["Error"] = "Document file not found.";
                    return RedirectToAction("Homepage", "Home");
                }

                // Check if the viewing file exists
                string fullPath = Path.Combine(_environment.WebRootPath, viewFilePath.TrimStart('/'));
                if (!System.IO.File.Exists(fullPath))
                {
                    TempData["Error"] = "Document file not found on disk.";
                    return RedirectToAction("Homepage", "Home");
                }

                // Increment view count
                document.ViewCount++;
                await _documentService.UpdateAsync(document);

                bool isFavorited = false;
                bool isLiked = false;
                if (User.Identity.IsAuthenticated)
                {
                    var currentUser = await _userManager.GetUserAsync(User);
                    if (currentUser != null)
                    {
                        // Pass document object to avoid re-querying and ensure CourseId is available
                        await AddOrUpdateRecentViewed(document, currentUser.Id);

                        // Kiểm tra xem đã favorite chưa
                        isFavorited = await _userFavoriteService.ExistsAsync(uf =>
                            uf.DocumentId == document.DocumentId && uf.UserId == currentUser.Id);

                        // Kiểm tra đã like chưa
                        isLiked = await _documentLikeService.ExistsAsync(dl =>
                            dl.DocumentId == document.DocumentId && dl.UserId == currentUser.Id);
                    }
                }
                // Get uploader statistics
                var uploaderStats = await GetUploaderStats(document.UploaderId);

                // Create view model
                var viewModel = new DetailDocumentViewModel
                {
                    Document = document,
                    UploaderStats = uploaderStats,
                    FileUrl = viewFilePath, // This will be the PDF path if available
                    FilePDFpath = document.FilePDFpath,
                    CanDownload = await CanUserDownload(document),
                    IsPremiumOnly = document.IsPremiumOnly, 
                    IsFavorited = isFavorited,
                    IsLiked = isLiked
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading document: {ex.Message}";
                return RedirectToAction("Homepage", "Home");
            }
        }

        private async Task AddOrUpdateRecentViewed(Document document, string userId)
        {
            if (string.IsNullOrEmpty(userId) || document == null)
                return;

            try
            {
                // Validate CourseId is available
                if (document.CourseId == 0)
                {
                    Console.WriteLine($"Warning: Document {document.DocumentId} has invalid CourseId (0). Cannot add RecentViewed.");
                    return;
                }

                // Kiểm tra xem user đã xem document này chưa
                var existingView = await _recentViewedService.GetAsync(rv =>
                    rv.UserId == userId && rv.DocumentId == document.DocumentId);

                if (existingView != null)
                {
                    // Nếu đã tồn tại, cập nhật thời gian xem và CourseId (nếu thay đổi)
                    existingView.ViewedAt = DateTime.UtcNow;
                    existingView.CourseId = document.CourseId; // Update CourseId in case it changed
                    await _recentViewedService.UpdateAsync(existingView);
                }
                else
                {
                    // Nếu chưa tồn tại, tạo mới với CourseId từ document
                    var recentView = new RecentViewed
                    {
                        DocumentId = document.DocumentId,
                        CourseId = document.CourseId, // Set CourseId from document - REQUIRED
                        UserId = userId,
                        ViewedAt = DateTime.UtcNow
                    };
                    await _recentViewedService.AddAsync(recentView);
                }
            }
            catch (Exception ex)
            {
                // Log error but don't throw - we don't want to break document viewing if RecentViewed fails
                Console.WriteLine($"Error adding/updating RecentViewed for DocumentId {document.DocumentId}, UserId {userId}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }
        }

        // Thêm vào DocumentController.cs

        [HttpGet]
        public async Task<IActionResult> AllDocuments(string searchTerm = "", string sortBy = "recent", int page = 1, int pageSize = 12)
        {
            try
            {
                // Lấy tất cả documents với eager loading
                var query = _documentService.GetAllQueryable()
                    .Where(d=>d.Status== "approved")
                    .Include(d => d.Course)
                    .Include(d => d.Uploader)
                    .Include(d => d.Uploader.University)
                    .AsQueryable();

                // Tìm kiếm nếu có
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    query = query.Where(d =>
                        d.Title.ToLower().Contains(searchTerm) ||
                        d.Description.ToLower().Contains(searchTerm) ||
                        d.Course.CourseName.ToLower().Contains(searchTerm));
                }

                // Sắp xếp
                query = sortBy.ToLower() switch
                {
                    "popular" => query.OrderByDescending(d => d.LikeCount),
                    "views" => query.OrderByDescending(d => d.ViewCount),
                    "downloads" => query.OrderByDescending(d => d.DownloadCount),
                    "title" => query.OrderBy(d => d.Title),
                    _ => query.OrderByDescending(d => d.UpdatedAt) // recent
                };

                // Tính tổng số documents
                int totalDocuments = await query.CountAsync();
                int totalPages = (int)Math.Ceiling(totalDocuments / (double)pageSize);

                // Phân trang
                var documents = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Kiểm tra liked documents nếu user đã đăng nhập
                Dictionary<int, bool> userLikedDocuments = new Dictionary<int, bool>();
                if (User.Identity.IsAuthenticated)
                {
                    var currentUser = await _userManager.GetUserAsync(User);
                    if (currentUser != null)
                    {
                        var documentIds = documents.Select(d => d.DocumentId).ToList();
                        var likedDocs = await _documentLikeService.GetAllQueryable(
                            dl => dl.UserId == currentUser.Id && documentIds.Contains(dl.DocumentId))
                            .Select(dl => dl.DocumentId)
                            .ToListAsync();

                        userLikedDocuments = documentIds.ToDictionary(
                            id => id,
                            id => likedDocs.Contains(id));
                    }
                }

                // Tạo view model
                var viewModel = new AllDocumentsViewModel
                {
                    Documents = documents,
                    UserLikedDocuments = userLikedDocuments,
                    CurrentPage = page,
                    TotalPages = totalPages,
                    SearchTerm = searchTerm,
                    SortBy = sortBy,
                    TotalDocuments = totalDocuments
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading documents: {ex.Message}";
                return RedirectToAction("Homepage", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> AllRecentVieweds(string searchTerm = "", string sortBy = "recent", int page = 1, int pageSize = 12)
        {
            try
            {
                // Kiểm tra user đã đăng nhập
                if (!User.Identity.IsAuthenticated)
                {
                    TempData["Error"] = "Please login to view your recent viewed documents.";
                    return RedirectToAction("Signin", "Auth");
                }

                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction("Homepage", "Home");
                }

                // Lấy tất cả RecentVieweds của user với eager loading
                var query = _recentViewedService.GetAllQueryable()
                    .Where(rv => rv.UserId == currentUser.Id && rv.Document != null && rv.Document.Status == "approved")
                    .Include(rv => rv.Document)
                        .ThenInclude(d => d.Course)
                    .Include(rv => rv.Document)
                        .ThenInclude(d => d.Uploader)
                            .ThenInclude(u => u.University)
                    .AsQueryable();

                // Tìm kiếm nếu có
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    query = query.Where(rv =>
                        rv.Document.Title.ToLower().Contains(searchTerm) ||
                        (rv.Document.Description != null && rv.Document.Description.ToLower().Contains(searchTerm)) ||
                        (rv.Document.Course != null && rv.Document.Course.CourseName.ToLower().Contains(searchTerm)));
                }

                // Sắp xếp
                query = sortBy.ToLower() switch
                {
                    "popular" => query.OrderByDescending(rv => rv.Document.LikeCount),
                    "views" => query.OrderByDescending(rv => rv.Document.ViewCount),
                    "downloads" => query.OrderByDescending(rv => rv.Document.DownloadCount),
                    "title" => query.OrderBy(rv => rv.Document.Title),
                    "oldest" => query.OrderBy(rv => rv.ViewedAt),
                    _ => query.OrderByDescending(rv => rv.ViewedAt) // recent (most recently viewed first)
                };

                // Tính tổng số RecentVieweds
                int totalRecentVieweds = await query.CountAsync();
                int totalPages = (int)Math.Ceiling(totalRecentVieweds / (double)pageSize);

                // Phân trang
                var recentVieweds = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Kiểm tra liked documents
                Dictionary<int, bool> userLikedDocuments = new Dictionary<int, bool>();
                if (recentVieweds.Any())
                {
                    var documentIds = recentVieweds.Where(rv => rv.Document != null).Select(rv => rv.Document.DocumentId).ToList();
                    var likedDocs = await _documentLikeService.GetAllQueryable(
                        dl => dl.UserId == currentUser.Id && documentIds.Contains(dl.DocumentId))
                        .Select(dl => dl.DocumentId)
                        .ToListAsync();

                    userLikedDocuments = documentIds.ToDictionary(
                        id => id,
                        id => likedDocs.Contains(id));
                }

                // Tạo view model
                var viewModel = new AllRecentViewedsViewModel
                {
                    RecentVieweds = recentVieweds,
                    UserLikedDocuments = userLikedDocuments,
                    CurrentPage = page,
                    TotalPages = totalPages,
                    SearchTerm = searchTerm,
                    SortBy = sortBy,
                    TotalRecentVieweds = totalRecentVieweds
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading recent viewed documents: {ex.Message}";
                return RedirectToAction("Homepage", "Home");
            }
        }

       


        public async Task<IActionResult> ViewFile(int id)
        {
            try
            {
                var document = await _documentService.GetAsync(d => d.DocumentId == id);
                if (document == null)
                {
                    return NotFound("Document not found");
                }

                // Use PDF version if available, otherwise use original file
                string filePath = GetViewFilePath(document);
                if (string.IsNullOrEmpty(filePath))
                {
                    return NotFound("File not available for viewing");
                }

                string fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
                if (!System.IO.File.Exists(fullPath))
                {
                    return NotFound("File not found on disk");
                }

                // Get file info for ETag and Last-Modified headers
                var fileInfo = new FileInfo(fullPath);

                // Check if client has cached version
                var etag = $"\"{fileInfo.LastWriteTime.Ticks}\"";
                if (Request.Headers.ContainsKey("If-None-Match") &&
                    Request.Headers["If-None-Match"].ToString() == etag)
                {
                    return StatusCode(304); // Not Modified
                }

                var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);

                // Determine content type based on actual file being served
                string contentType = GetContentTypeForFile(filePath, document);

                // Get file extension for the file being served
                string fileExtension = Path.GetExtension(filePath).TrimStart('.');

                // Add headers for better browser compatibility
                Response.Headers.Add("Content-Disposition", $"inline; filename=\"{document.Title}.{fileExtension}\"");
                Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
                Response.Headers.Add("Access-Control-Allow-Origin", "*");
                Response.Headers.Add("Cache-Control", "public, max-age=3600");
                Response.Headers.Add("ETag", etag);
                Response.Headers.Add("Last-Modified", fileInfo.LastWriteTime.ToString("R"));

                return File(fileBytes, contentType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error serving file: {ex.Message}");
            }
        }

        // Download action
        [HttpGet]
        //[ValidateAntiForgeryToken]

        public async Task<IActionResult> Download(int id)
        {
            try
            {
                var document = await _documentService.GetAsync(d => d.DocumentId == id);
                if (document == null)
                {
                    TempData["Error"] = "Document not found.";
                    return RedirectToAction("DetailDocument", new { id });
                }

                // Check if user can download
                if (!await CanUserDownload(document))
                {
                    TempData["Error"] = " Insufficient points to download this document.";
                    return RedirectToAction("DetailDocument", new { id });
                }

                // Always use the original file for download
                string filePath = Path.Combine(_environment.WebRootPath, document.FilePath.TrimStart('/'));
                if (!System.IO.File.Exists(filePath))
                {
                    TempData["Error"] = "File not found on server.";
                    return RedirectToAction("DetailDocument", new { id });
                }
                // Increment download count and deduct points
                document.DownloadCount++;
                await _documentService.UpdateAsync(document);
                await DeductDownloadPoints(document);

                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                string contentType = document.FileType.ToLower() switch
                {
                    "pdf" => "application/pdf",
                    "doc" => "application/msword",
                    "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    "txt" => "text/plain",
                    _ => "application/octet-stream"
                };

                return File(fileBytes, contentType, $"{document.Title}.{document.FileType}");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }



        // Thêm phương thức ToggleFavorite
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFavorite(int id)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return Json(new { success = false, message = "Please login to favorite documents" });
                }

                var document = await _documentService.GetAsync(d => d.DocumentId == id);
                if (document == null)
                {
                    return Json(new { success = false, message = "Document not found" });
                }

                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // Kiểm tra xem user đã favorite document này chưa
                var existingFavorite = await _userFavoriteService.GetAsync(uf =>
                    uf.DocumentId == id && uf.UserId == currentUser.Id);

                bool isFavorited = false;

                if (existingFavorite != null)
                {
                    // User đã favorite rồi -> Remove favorite
                    await _userFavoriteService.DeleteAsync(existingFavorite.Id);
                    isFavorited = false;
                }
                else
                {
                    // User chưa favorite -> Add favorite
                    var newFavorite = new UserFavorite
                    {
                        DocumentId = id,
                        UserId = currentUser.Id,
                        CreatedAt = DateTime.Now
                    };
                    await _userFavoriteService.AddAsync(newFavorite);
                    isFavorited = true;
                }

                return Json(new
                {
                    success = true,
                    isFavorited = isFavorited,
                    message = isFavorited ? "Added to favorites" : "Removed from favorites"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLike(int id)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return Json(new { success = false, message = "Please login to like documents" });
                }

                var document = await _documentService.GetAsync(d => d.DocumentId == id);
                if (document == null)
                {
                    return Json(new { success = false, message = "Document not found" });
                }

                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // Kiểm tra xem user đã like document này chưa
                var existingLike = await _documentLikeService.GetAsync(dl =>
                    dl.DocumentId == id && dl.UserId == currentUser.Id);

                bool isLiked = false;
                int newLikeCount = 0;

                if (existingLike != null)
                {
                    // User đã like rồi -> Unlike
                    await _documentLikeService.DeleteAsync(existingLike.Id);
                    document.LikeCount = Math.Max(0, document.LikeCount - 1);
                    isLiked = false;
                }
                else
                {
                    // User chưa like -> Like
                    var newLike = new DocumentLike
                    {
                        DocumentId = id,
                        UserId = currentUser.Id,
                        LikedAt = DateTime.Now
                    };
                    await _documentLikeService.AddAsync(newLike);
                    document.LikeCount++;
                    isLiked = true;
                }

                await _documentService.UpdateAsync(document);
                newLikeCount = document.LikeCount;

                return Json(new
                {
                    success = true,
                    isLiked = isLiked,
                    likeCount = newLikeCount
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Helper methods
        private async Task<UploaderStatsViewModel> GetUploaderStats(string uploaderId)
        {
            var uploader = await _userService.GetAllQueryable(u => u.Id == uploaderId)
                .Include(u => u.University)
                .FirstOrDefaultAsync();

            if (uploader == null)
                return new UploaderStatsViewModel();

            var documentsCount = _documentService.GetAllQueryable(d => d.UploaderId == uploaderId).Count();
            var totalLikes = _documentService.GetAllQueryable(d => d.UploaderId == uploaderId)
                .Sum(d => d.LikeCount);

            return new UploaderStatsViewModel
            {
                UploaderId = uploaderId,
                FullName = uploader.FullName ?? "Unknown User",
                AvatarUrl = uploader.AvatarUrl,
                UniversityName = uploader.University?.Name ?? "Unknown University",
                FollowerCount = 0, // Implement follower system later
                UploadCount = documentsCount,
                LikeCount = totalLikes
            };
        }

        private async Task<bool> CanUserDownload(Document document)
        {
            if (!User.Identity.IsAuthenticated) return false;
            if (document.PointsToDownload == 0) return true;

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return false;

            // Check if user is the uploader
            if (currentUser.Id == document.UploaderId) return true;

            // Check if user has enough points
            return currentUser.PointsBalance >= document.PointsToDownload;
        }

        private async Task DeductDownloadPoints(Document document)
        {
            if (document.PointsToDownload > 0 && User.Identity.IsAuthenticated)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null && currentUser.Id != document.UploaderId)
                {
                    currentUser.PointsBalance -= document.PointsToDownload;
                    await _userManager.UpdateAsync(currentUser);
                }
            }
        }

        // Helper method to determine which file to use for viewing
        private string GetViewFilePath(Document document)
        {
            // Priority: Use PDF version if available and exists, otherwise use original
            if (!string.IsNullOrEmpty(document.FilePDFpath))
            {
                string pdfFullPath = Path.Combine(_environment.WebRootPath, document.FilePDFpath.TrimStart('/'));
                if (System.IO.File.Exists(pdfFullPath))
                {
                    return document.FilePDFpath;
                }
            }

            // Fallback to original file
            return document.FilePath;
        }

        // Helper method to get content type for the actual file being served
        private string GetContentTypeForFile(string filePath, Document document)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();

            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".txt" => "text/plain",
                ".rtf" => "application/rtf",
                _ => "application/octet-stream"
            };
        }

        // Helper method to check if we're serving the PDF version
        private bool IsServingPdfVersion(Document document)
        {
            return !string.IsNullOrEmpty(document.FilePDFpath) &&
                   System.IO.File.Exists(Path.Combine(_environment.WebRootPath, document.FilePDFpath.TrimStart('/')));
        }

        [HttpGet]
        public async Task<IActionResult> GetDocumentFileForAI(int id)
        {
            try
            {
                var document = await _documentService.GetAsync(d => d.DocumentId == id);
                if (document == null)
                {
                    return NotFound(new { error = "Document not found" });
                }

                // Get the file path (prefer PDF if available)
                string filePath = GetViewFilePath(document);
                if (string.IsNullOrEmpty(filePath))
                {
                    return NotFound(new { error = "Document file not found" });
                }

                string fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
                if (!System.IO.File.Exists(fullPath))
                {
                    return NotFound(new { error = "File not found on disk" });
                }

                // Get file info
                var fileInfo = new FileInfo(fullPath);
                var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);

                // Determine content type
                string contentType = GetContentTypeForFile(filePath, document);
                string extension = Path.GetExtension(filePath).ToLowerInvariant();

                // Return file with proper headers
                Response.Headers.Append("X-Document-Title", document.Title);
                Response.Headers.Append("X-Document-Id", document.DocumentId.ToString());

                return File(fileBytes, contentType, $"{document.Title}{extension}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Error retrieving document: {ex.Message}" });
            }
        }
        // View Models
        public class DocumentDetailViewModel
        {
            public Document Document { get; set; }
            public UploaderStatsViewModel UploaderStatus { get; set; }
            public string FileUrl { get; set; }
            public bool CanDownload { get; set; }
            public List<CommentViewModel> Comments { get; set; } = new List<CommentViewModel>();
        }

        public class UploaderStatsViewModel
        {
            public string UploaderId { get; set; }
            public string FullName { get; set; }
            public string AvatarUrl { get; set; }
            public string UniversityName { get; set; }
            public int FollowerCount { get; set; }
            public int UploadCount { get; set; }
            public int LikeCount { get; set; }
        }

        public class CommentViewModel
        {
            public int CommentId { get; set; }
            public string UserName { get; set; }
            public string Content { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}