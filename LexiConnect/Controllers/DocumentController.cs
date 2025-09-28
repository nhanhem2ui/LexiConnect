using BusinessObjects;
using LexiConnect.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories;

namespace LexiConnect.Controllers
{
    public class DocumentController : Controller
    {
        private readonly IGenericRepository<Document> _documentRepository;
        private readonly IGenericRepository<Course> _courseRepository;
        private readonly IGenericRepository<Users> _userRepository;
        private readonly UserManager<Users> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly IGenericRepository<DocumentLike> _documentLikeRepository;

        public DocumentController(
            IGenericRepository<Document> documentRepository,
            IGenericRepository<Course> courseRepository,
            IGenericRepository<Users> userRepository,
            IGenericRepository<DocumentLike> documentLikeRepository,
            UserManager<Users> userManager,
            IWebHostEnvironment environment)
        {
            _documentRepository = documentRepository;
            _courseRepository = courseRepository;
            _userRepository = userRepository;
            _userManager = userManager;
            _environment = environment;
            _documentRepository = documentRepository;   
            _documentLikeRepository = documentLikeRepository;
        }

        

        // GET: Document/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                // Get document with eager loading for related entities
                var document = await _documentRepository.GetAllQueryable(d => d.DocumentId == id)
                    .Include(d => d.Course)
                    .Include(d => d.Uploader)
                    .Include(d => d.Uploader.University)
                    .FirstOrDefaultAsync();

                if (document == null)
                {
                    TempData["Error"] = "Document not found.";
                    return RedirectToAction("Index", "Home");
                }

                // Determine which file to use for viewing (PDF version if available)
                string viewFilePath = GetViewFilePath(document);
                if (string.IsNullOrEmpty(viewFilePath))
                {
                    TempData["Error"] = "Document file not found.";
                    return RedirectToAction("Index", "Home");
                }

                // Check if the viewing file exists
                string fullPath = Path.Combine(_environment.WebRootPath, viewFilePath.TrimStart('/'));
                if (!System.IO.File.Exists(fullPath))
                {
                    TempData["Error"] = "Document file not found on disk.";
                    return RedirectToAction("Index", "Home");
                }

                // Increment view count
                document.ViewCount++;
                await _documentRepository.UpdateAsync(document);

                // Get uploader statistics
                var uploaderStats = await GetUploaderStats(document.UploaderId);

                // Create view model
                var viewModel = new DetailDocumentViewModel
                {
                    Document = document,
                    UploaderStats = uploaderStats,
                    FileUrl = viewFilePath, // This will be the PDF path if available
                    FilePDFpath = document.FilePDFpath,
                    CanDownload = await CanUserDownload(document)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading document: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

       

        public async Task<IActionResult> ViewFile(int id)
        {
            try
            {
                var document = await _documentRepository.GetAsync(d => d.DocumentId == id);
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Download(int id)
        {
            try
            {
                var document = await _documentRepository.GetAsync(d => d.DocumentId == id);
                if (document == null)
                {
                    return Json(new { success = false, message = "Document not found" });
                }

                // Check if user can download
                if (!await CanUserDownload(document))
                {
                    return Json(new { success = false, message = "Insufficient points to download" });
                }

                // For download, always use the original file
                string filePath = Path.Combine(_environment.WebRootPath, document.FilePath.TrimStart('/'));
                if (!System.IO.File.Exists(filePath))
                {
                    return Json(new { success = false, message = "Original file not found" });
                }

                // Increment download count and deduct points
                document.DownloadCount++;
                await _documentRepository.UpdateAsync(document);
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

                var document = await _documentRepository.GetAsync(d => d.DocumentId == id);
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
                var existingLike = await _documentLikeRepository.GetAsync(dl =>
                    dl.DocumentId == id && dl.UserId == currentUser.Id);

                bool isLiked = false;
                int newLikeCount = 0;

                if (existingLike != null)
                {
                    // User đã like rồi -> Unlike
                    await _documentLikeRepository.DeleteAsync(existingLike.Id);
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
                    await _documentLikeRepository.AddAsync(newLike);
                    document.LikeCount++;
                    isLiked = true;
                }

                await _documentRepository.UpdateAsync(document);
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
            var uploader = await _userRepository.GetAllQueryable(u => u.Id == uploaderId)
                .Include(u => u.University)
                .FirstOrDefaultAsync();

            if (uploader == null)
                return new UploaderStatsViewModel();

            var documentsCount = _documentRepository.GetAllQueryable(d => d.UploaderId == uploaderId).Count();
            var totalLikes = _documentRepository.GetAllQueryable(d => d.UploaderId == uploaderId)
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