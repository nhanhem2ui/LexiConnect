using BusinessObjects;
using LexiConnect.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories;
using System.ComponentModel.DataAnnotations;

namespace LexiConnect.Controllers
{


    public class UploadController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IGenericRepository<Document> _documentRepository;
        private readonly IGenericRepository<Course> _courseRepository;

        private readonly UserManager<Users> _userManager;
        private readonly long _maxFileSize = 10 * 1024 * 1024; // 10MB
        private readonly string[] _allowedExtensions = { ".pdf", ".doc", ".docx" };

        public UploadController(IWebHostEnvironment environment, IGenericRepository<Document> documentRepository, UserManager<Users> userManager, IGenericRepository<Course> courseRepository)
        {
            _environment = environment;
            _documentRepository = documentRepository;
            _userManager = userManager;
            _courseRepository = courseRepository;

        }

        // GET: Upload
        public IActionResult Index()
        {
            ViewBag.MaxFileSize = _maxFileSize;
            ViewBag.AllowedExtensions = string.Join(", ", _allowedExtensions);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadDocuments(List<IFormFile> files)
        {
            var uploadResults = new List<UploadResult>();

            try
            {
                // Check if user is authenticated
                var currentUser = await _userManager.GetUserAsync(User);
                if (!User.Identity.IsAuthenticated)
                {
                    TempData["Error"] = "You must be logged in to upload documents.";
                    return RedirectToAction("Signin", "Auth");
                }

                // Validate if files are provided
                if (files == null || !files.Any())
                {
                    TempData["Error"] = "Please select at least one file to upload.";
                    return RedirectToAction("Index");
                }



                // Create upload directories if they don't exist
                string uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "documents");
                string thumbnailPath = Path.Combine(_environment.WebRootPath, "uploads", "thumbnails");

                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }
                if (!Directory.Exists(thumbnailPath))
                {
                    Directory.CreateDirectory(thumbnailPath);
                }

                // Process each file
                foreach (var file in files)
                {
                    var result = await ProcessSingleFile(file, uploadPath, thumbnailPath, currentUser.Id);
                    uploadResults.Add(result);
                }

                // Check if all files were uploaded successfully
                var successfulUploads = uploadResults.Where(r => r.Success).ToList();
                var failedUploads = uploadResults.Where(r => !r.Success).ToList();

                if (successfulUploads.Any())
                {
                    // Store successful uploads in TempData for next step
                    TempData["SuccessfulUploads"] = successfulUploads.Count;
                    TempData["UploadedFiles"] = string.Join(", ", successfulUploads.Select(f => f.FileName));
                    TempData["DocumentIds"] = string.Join(",", successfulUploads.Select(f => f.DocumentId));

                    if (failedUploads.Any())
                    {
                        TempData["Warning"] = $"{failedUploads.Count} files failed to upload: {string.Join(", ", failedUploads.Select(f => f.ErrorMessage))}";
                    }

                    return RedirectToAction("Details");
                }
                else
                {
                    TempData["Error"] = "No  files were successfully uploaded. Please try again.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred during upload: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            Console.WriteLine($"DeleteDocument called with id = {id}");

            try
            {
                var document = await _documentRepository.GetAsync(d => d.DocumentId == id);
                if (document == null)
                    return Json(new { success = false, error = "Document not found" });

                // Delete the physical file if it exists
                if (!string.IsNullOrEmpty(document.FilePath))
                {
                    string fullPath = Path.Combine(_environment.WebRootPath, document.FilePath.TrimStart('/'));
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                }

                // Delete from database
                await _documentRepository.DeleteAsync(document.DocumentId);

                // Update TempData to remove the deleted document ID
                if (TempData["DocumentIds"] != null)
                {
                    var documentIdsString = TempData["DocumentIds"].ToString();
                    var documentIds = documentIdsString.Split(',').Select(int.Parse).ToList();
                    documentIds.Remove(id);

                    if (documentIds.Any())
                    {
                        TempData["DocumentIds"] = string.Join(",", documentIds);
                        TempData.Keep("DocumentIds"); // Keep for the next request
                    }
                    else
                    {
                        TempData.Remove("DocumentIds");
                    }
                }

                Console.WriteLine($"Document {id} deleted successfully");
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting document {id}: {ex.Message}");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details()
        {
            if (TempData["DocumentIds"] == null)
            {
                TempData["Info"] = "No documents found. Please upload some documents first.";
                return RedirectToAction("Index");
            }

            try
            {
                // Get document IDs from TempData
                var documentIdsString = TempData["DocumentIds"].ToString();
                var documentIds = documentIdsString.Split(',').Where(id => !string.IsNullOrWhiteSpace(id)).Select(int.Parse).ToList();

                // Check if we have any valid document IDs
                if (!documentIds.Any())
                {
                    TempData["Info"] = "No valid documents found. Please upload some documents first.";
                    return RedirectToAction("Index");
                }

                // Get documents from database
                var documents = new List<SingleDocumentModel>();
                foreach (var docId in documentIds)
                {
                    var document = await _documentRepository.GetAsync(d => d.DocumentId == docId);
                    if (document != null)
                    {
                        documents.Add(new SingleDocumentModel
                        {
                            DocumentId = document.DocumentId,
                            FileName = Path.GetFileName(document.Title),
                            Title = document.Title,
                            DocumentType = document.DocumentType,
                            Description = document.Description,
                            PublicYear = DateTime.Now
                        });
                    }
                }

                // Check if we found any documents
                if (!documents.Any())
                {
                    TempData["Warning"] = "No documents were found in the database. They may have been deleted.";
                    return RedirectToAction("Index");
                }

                var model = new DocumentDetailsViewModel { Documents = documents };

                ViewBag.SuccessfulUploads = TempData["SuccessfulUploads"];
                ViewBag.UploadedFiles = TempData["UploadedFiles"];
                TempData.Keep("DocumentIds"); // Keep for the next request

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading document details: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDetails(DocumentDetailsViewModel model)
        {
            // Check if we have any documents to process
            if (model.Documents == null || !model.Documents.Any())
            {
                TempData["Error"] = "No documents found to save. Please upload some documents first.";
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    int updatedCount = 0;
                    var errors = new List<string>();

                    // Update documents with additional details
                    foreach (var documentModel in model.Documents)
                    {
                        var document = await _documentRepository.GetAsync(d => d.DocumentId == documentModel.DocumentId);
                        if (document != null)
                        {
                            // Update document with form data
                            document.Title = documentModel.Title;
                            document.Description = documentModel.Description;
                            document.DocumentType = documentModel.DocumentType;
                            document.CourseId = documentModel.CourseId;
                            document.UpdatedAt = DateTime.UtcNow;

                            // Set points based on document type
                            document.PointsAwarded = GetPointsForDocumentType(document.DocumentType);
                            document.PointsToDownload = GetDownloadPointsForDocumentType(document.DocumentType);

                            // Update status to pending for review
                            document.Status = "pending";

                            await _documentRepository.UpdateAsync(document);
                            updatedCount++;
                        }
                        else
                        {
                            errors.Add($"Document '{documentModel.FileName}' was not found in the database.");
                        }
                    }

                    if (updatedCount > 0)
                    {
                        TempData["Success"] = $"Your documents have been uploaded successfully! {updatedCount} document(s) processed.";
                        if (errors.Any())
                        {
                            TempData["Warning"] = "Some documents could not be processed: " + string.Join(", ", errors);
                        }
                        return RedirectToAction("Complete");
                    }
                    else
                    {
                        TempData["Error"] = "No documents could be processed. " + string.Join(", ", errors);
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error saving document details: {ex.Message}";
                    return View(model);
                }
            }

            return View("Details", model);
        }

        // GET: Upload/Complete
        public IActionResult Complete()
        {
            return View();
        }

        // Helper method to process individual files
        private async Task<UploadResult> ProcessSingleFile(IFormFile file, string uploadPath, string thumbnailPath, string uploaderId)
        {
            var result = new UploadResult { FileName = file.FileName };

            try
            {
                // Validate file
                var validationResult = ValidateFile(file);
                if (!validationResult.IsValid)
                {
                    result.Success = false;
                    result.ErrorMessage = validationResult.ErrorMessage;
                    return result;
                }

                // Generate unique filename
                string fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                string uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                string filePath = Path.Combine(uploadPath, uniqueFileName);

                // Save file to disk
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Create Document entity
                var document = new Document
                {
                    Title = Path.GetFileNameWithoutExtension(file.FileName),
                    Description = $"Uploaded document: {file.FileName}",
                    DocumentType = DetermineDocumentType(file.FileName),
                    FilePath = $"/uploads/documents/{uniqueFileName}",
                    FileSize = file.Length,
                    FileType = fileExtension.TrimStart('.'),
                    UploaderId = uploaderId,
                    CourseId = 1, // Default course - will be updated in Details step
                    Status = "pending",
                    LanguageCode = "en",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    PageCount = await EstimatePageCount(filePath, fileExtension),
                    PointsAwarded = 0, // Will be set based on document type in Details
                    PointsToDownload = 0, // Will be set based on document type in Details
                    IsPremiumOnly = false,
                    IsAiGenerated = false
                };

                // Generate thumbnail if it's a PDF
                if (fileExtension == ".pdf")
                {
                    document.ThumbnailUrl = await GeneratePdfThumbnail(filePath, thumbnailPath, uniqueFileName);
                }

                // Save to database
                bool saved = await _documentRepository.AddAsync(document);
                if (saved)
                {
                    result.Success = true;
                    result.FilePath = filePath;
                    result.UniqueFileName = uniqueFileName;
                    result.FileSize = file.Length;
                    result.DocumentId = document.DocumentId;
                }
                else
                {
                    result.Success = false;
                    result.ErrorMessage = "Failed to save document to database.";

                    // Clean up uploaded file if database save failed
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Error uploading {file.FileName}: {ex.Message}";
            }

            return result;
        }

        // Helper method to validate files
        private FileValidationResult ValidateFile(IFormFile file)
        {
            // Check if file is empty
            if (file.Length == 0)
            {
                return new FileValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"{file.FileName} is empty."
                };
            }

            // Check file size
            if (file.Length > _maxFileSize)
            {
                return new FileValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"{file.FileName} exceeds maximum size of {_maxFileSize / 1024 / 1024}MB."
                };
            }

            // Check file extension
            string fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(fileExtension))
            {
                return new FileValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"{file.FileName} has invalid file type. Only PDF, DOC, and DOCX files are allowed."
                };
            }

            return new FileValidationResult { IsValid = true };
        }

        // Helper method to determine document type based on filename
        private string DetermineDocumentType(string fileName)
        {
            string lowerFileName = fileName.ToLowerInvariant();

            if (lowerFileName.Contains("assignment") || lowerFileName.Contains("hw"))
                return "assignment";
            else if (lowerFileName.Contains("exam") || lowerFileName.Contains("test"))
                return "exam";
            else if (lowerFileName.Contains("quiz"))
                return "quiz";
            else if (lowerFileName.Contains("guide") || lowerFileName.Contains("study"))
                return "study_guide";
            else if (lowerFileName.Contains("flashcard") || lowerFileName.Contains("flash"))
                return "flashcards";
            else
                return "notes"; // default
        }

        // Helper method to get points awarded for document type
        private int GetPointsForDocumentType(string documentType)
        {
            return documentType switch
            {
                "exam" => 15,
                "assignment" => 10,
                "quiz" => 8,
                "study_guide" => 12,
                "flashcards" => 6,
                "notes" => 5,
                _ => 5
            };
        }

        // Helper method to get download points required for document type
        private int GetDownloadPointsForDocumentType(string documentType)
        {
            return documentType switch
            {
                "exam" => 8,
                "assignment" => 5,
                "quiz" => 4,
                "study_guide" => 6,
                "flashcards" => 3,
                "notes" => 2,
                _ => 2
            };
        }

        // Helper method to estimate page count (basic implementation)
        private async Task<int?> EstimatePageCount(string filePath, string fileExtension)
        {
            try
            {
                if (fileExtension == ".pdf")
                {
                    // For PDF files, you could use a PDF library like iTextSharp
                    // This is a placeholder - implement actual PDF page counting
                    var fileInfo = new System.IO.FileInfo(filePath);
                    return Math.Max(1, (int)(fileInfo.Length / 50000)); // Rough estimate
                }
                else
                {
                    // For DOC/DOCX files, you could use DocumentFormat.OpenXml
                    // This is a placeholder
                    var fileInfo = new System.IO.FileInfo(filePath);
                    return Math.Max(1, (int)(fileInfo.Length / 30000)); // Rough estimate
                }
            }
            catch
            {
                return null;
            }
        }

        // Helper method to generate PDF thumbnail (placeholder)
        private async Task<string?> GeneratePdfThumbnail(string pdfPath, string thumbnailPath, string uniqueFileName)
        {
            try
            {
                // This is a placeholder - you would use a library like ImageSharp or Magick.NET
                // to generate actual thumbnails from PDF first page
                string thumbnailFileName = $"{Path.GetFileNameWithoutExtension(uniqueFileName)}_thumb.jpg";
                return $"/uploads/thumbnails/{thumbnailFileName}";
            }
            catch
            {
                return null;
            }
        }

        // API endpoint to get upload progress (for future enhancement)
        [HttpGet]
        public IActionResult GetUploadProgress()
        {
            return Json(new { progress = 100, status = "complete" });
        }
    }

    // Helper classes
    public class UploadResult
    {
        public string FileName { get; set; } = string.Empty;
        public string UniqueFileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public int DocumentId { get; set; }
    }

    public class FileValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }



    public class SingleDocumentModel
    {
        public int DocumentId { get; set; }
        public string FileName { get; set; } = string.Empty;

        [Required(ErrorMessage = "University is required")]
        public string University { get; set; } = string.Empty;

        [Required(ErrorMessage = "Course is required")]
        public int CourseId { get; set; }

        public string CourseName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Title is required")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Document type is required")]
        public string DocumentType { get; set; } = string.Empty;

        public DateTime PublicYear { get; set; } = DateTime.Now;

        [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }
    }
}