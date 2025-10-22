using BusinessObjects;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using LexiConnect.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Repositories;
using System.ComponentModel.DataAnnotations;
using Document = BusinessObjects.Document;


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
                    TempData["Error"] = "Please select at least one file  to upload.";
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

                // Delete the main physical file if it exists
                if (!string.IsNullOrEmpty(document.FilePath))
                {
                    string fullPath = Path.Combine(_environment.WebRootPath, document.FilePath.TrimStart('/'));
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                        Console.WriteLine($"Main file deleted: {fullPath}");
                    }
                }

                // Delete the PDF file if it exists
                if (!string.IsNullOrEmpty(document.FilePDFpath))
                {
                    string pdfPath = Path.Combine(_environment.WebRootPath, document.FilePDFpath.TrimStart('/'));
                    if (System.IO.File.Exists(pdfPath))
                    {
                        System.IO.File.Delete(pdfPath);
                        Console.WriteLine($"PDF file deleted: {pdfPath}");
                    }
                }

                // Delete thumbnail if it exists
                if (!string.IsNullOrEmpty(document.ThumbnailUrl))
                {
                    string thumbnailPath = Path.Combine(_environment.WebRootPath, document.ThumbnailUrl.TrimStart('/'));
                    if (System.IO.File.Exists(thumbnailPath))
                    {
                        System.IO.File.Delete(thumbnailPath);
                        Console.WriteLine($"Thumbnail deleted: {thumbnailPath}");
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
                // Get all courses for the dropdown
                var courses = _courseRepository.GetAllQueryable().Select(c => new SelectListItem
                {
                    Value = c.CourseId.ToString(),
                    Text = $"{c.CourseCode} - {c.CourseName}"
                }).ToList();

                // Add default option at the beginning
                courses.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "Select a course",
                    Selected = true
                });

                var model = new DocumentDetailsViewModel { Documents = documents, Courses = courses };

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


        //Helper method to process individual files
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

                // Generate PDF path
                string pdfPath = await GeneratePDFPath(filePath, fileExtension, uniqueFileName);

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
                    IsAiGenerated = false,
                    FilePDFpath = pdfPath // Set the PDF path here
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

                    // Clean up PDF file if it was created
                    if (!string.IsNullOrEmpty(pdfPath))
                    {
                        string fullPdfPath = Path.Combine(_environment.WebRootPath, pdfPath.TrimStart('/'));
                        if (System.IO.File.Exists(fullPdfPath))
                        {
                            System.IO.File.Delete(fullPdfPath);
                        }
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

        // Helper method to generate PDF path
        private async Task<string> GeneratePDFPath(string originalFilePath, string fileExtension, string uniqueFileName)
        {
            try
            {
                // Create PDF directory if it doesn't exist
                string pdfPath = Path.Combine(_environment.WebRootPath, "uploads", "pdf");
                if (!Directory.Exists(pdfPath))
                {
                    Directory.CreateDirectory(pdfPath);
                }

                string pdfFileName = $"{Path.GetFileNameWithoutExtension(uniqueFileName)}.pdf";
                string fullPdfPath = Path.Combine(pdfPath, pdfFileName);

                if (fileExtension == ".pdf")
                {
                    // If it's already a PDF, just copy it to the PDF directory
                    System.IO.File.Copy(originalFilePath, fullPdfPath, true);
                }
                else if (fileExtension == ".doc" || fileExtension == ".docx")
                {
                    // For DOC/DOCX files, convert them to PDF
                    await ConvertDocumentToPDF(originalFilePath, fullPdfPath, fileExtension);
                }

                return $"/uploads/pdf/{pdfFileName}";
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the entire upload
                Console.WriteLine($"Error generating PDF path: {ex.Message}");
                return string.Empty;
            }
        }

        // Helper method to convert DOC/DOCX to PDF using DocumentFormat.OpenXml + iTextSharp
        private async Task ConvertDocumentToPDF(string inputPath, string outputPath, string fileExtension)
        {
            try
            {
                if (fileExtension == ".docx")
                {
                    await ConvertDocxToPdf(inputPath, outputPath);
                }
                else if (fileExtension == ".doc")
                {
                    // For .doc files, you might need additional libraries or conversion
                    // For now, we'll create a basic PDF with a message
                    await CreateDocConversionPdf(inputPath, outputPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error converting document to PDF: {ex.Message}");
                // Create a fallback PDF with error message
                await CreateErrorPdf(outputPath, Path.GetFileName(inputPath), ex.Message);
            }
        }

        // Convert DOCX to PDF using DocumentFormat.OpenXml + iTextSharp
        private async Task ConvertDocxToPdf(string docxPath, string pdfPath)
        {
            await Task.Run(() =>
            {
                using (var wordDocument = WordprocessingDocument.Open(docxPath, false))
                {
                    var body = wordDocument.MainDocumentPart.Document.Body;

                    using (var writer = new PdfWriter(pdfPath))
                    using (var pdf = new PdfDocument(writer))
                    {
                        var document = new iText.Layout.Document(pdf);
                        var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                        // Extract text from DOCX and add to PDF
                        foreach (var paragraph in body.Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
                        {
                            var text = paragraph.InnerText;
                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                var pdfParagraph = new iText.Layout.Element.Paragraph(text)
                                    .SetFont(font)
                                    .SetFontSize(12);

                                document.Add(pdfParagraph);
                            }
                        }

                        // Add some spacing if document is empty
                        if (!body.Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>().Any(p => !string.IsNullOrWhiteSpace(p.InnerText)))
                        {
                            document.Add(new iText.Layout.Element.Paragraph("Document content could not be extracted.")
                                .SetFont(font)
                                .SetFontSize(12));
                        }

                        document.Close();
                    }
                }
            });
        }

        // Create PDF for .doc files (since OpenXml doesn't support .doc format directly)
        private async Task CreateDocConversionPdf(string docPath, string pdfPath)
        {
            await Task.Run(() =>
            {
                using (var writer = new PdfWriter(pdfPath))
                using (var pdf = new PdfDocument(writer))
                {
                    var document = new iText.Layout.Document(pdf);
                    var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                    // Add header
                    document.Add(new iText.Layout.Element.Paragraph("Document Conversion Notice")
                        .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                        .SetFontSize(16)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                    document.Add(new iText.Layout.Element.Paragraph("\n"));

                    // Add content
                    document.Add(new iText.Layout.Element.Paragraph($"Original file: {Path.GetFileName(docPath)}")
                        .SetFont(font)
                        .SetFontSize(12));

                    document.Add(new iText.Layout.Element.Paragraph("File type: Microsoft Word Document (.doc)")
                        .SetFont(font)
                        .SetFontSize(12));

                    document.Add(new iText.Layout.Element.Paragraph($"Converted on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                        .SetFont(font)
                        .SetFontSize(12));

                    document.Add(new iText.Layout.Element.Paragraph("\n"));

                    document.Add(new iText.Layout.Element.Paragraph("Note: .DOC files require Microsoft Office or LibreOffice for full content extraction. This is a placeholder PDF. For complete conversion, consider upgrading to .DOCX format or implementing Office automation.")
                        .SetFont(font)
                        .SetFontSize(10)
                        .SetFontColor(iText.Kernel.Colors.ColorConstants.GRAY));

                    document.Close();
                }
            });
        }

        // Create error PDF when conversion fails
        private async Task CreateErrorPdf(string pdfPath, string originalFileName, string errorMessage)
        {
            await Task.Run(() =>
            {
                using (var writer = new PdfWriter(pdfPath))
                using (var pdf = new PdfDocument(writer))
                {
                    var document = new iText.Layout.Document(pdf);
                    var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                    document.Add(new iText.Layout.Element.Paragraph("Document Conversion Error")
                        .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                        .SetFontSize(16)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetFontColor(iText.Kernel.Colors.ColorConstants.RED));

                    document.Add(new iText.Layout.Element.Paragraph("\n"));

                    document.Add(new iText.Layout.Element.Paragraph($"Original file: {originalFileName}")
                        .SetFont(font)
                        .SetFontSize(12));

                    document.Add(new iText.Layout.Element.Paragraph($"Error occurred: {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                        .SetFont(font)
                        .SetFontSize(12));

                    document.Add(new iText.Layout.Element.Paragraph("\n"));

                    document.Add(new iText.Layout.Element.Paragraph("Error details:")
                        .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                        .SetFontSize(12));

                    document.Add(new iText.Layout.Element.Paragraph(errorMessage)
                        .SetFont(font)
                        .SetFontSize(10)
                        .SetFontColor(iText.Kernel.Colors.ColorConstants.DARK_GRAY));

                    document.Close();
                }
            });
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



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CleanupTempFiles(string documentIds)
        {
            try
            {
                if (string.IsNullOrEmpty(documentIds))
                    return Json(new { success = true, message = "No files to cleanup" });

                var ids = documentIds.Split(',')
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Select(id => int.TryParse(id, out int result) ? result : 0)
                    .Where(id => id > 0)
                    .ToList();

                int cleanedCount = 0;
                foreach (var id in ids)
                {
                    var document = await _documentRepository.GetAsync(d => d.DocumentId == id);
                    if (document != null && document.Status == "pending") // Only cleanup pending documents
                    {
                        // Delete main physical file
                        if (!string.IsNullOrEmpty(document.FilePath))
                        {
                            string fullPath = Path.Combine(_environment.WebRootPath, document.FilePath.TrimStart('/'));
                            if (System.IO.File.Exists(fullPath))
                            {
                                System.IO.File.Delete(fullPath);
                            }
                        }

                        // Delete PDF file
                        if (!string.IsNullOrEmpty(document.FilePDFpath))
                        {
                            string pdfPath = Path.Combine(_environment.WebRootPath, document.FilePDFpath.TrimStart('/'));
                            if (System.IO.File.Exists(pdfPath))
                            {
                                System.IO.File.Delete(pdfPath);
                            }
                        }

                        // Delete thumbnail if exists
                        if (!string.IsNullOrEmpty(document.ThumbnailUrl))
                        {
                            string thumbnailPath = Path.Combine(_environment.WebRootPath, document.ThumbnailUrl.TrimStart('/'));
                            if (System.IO.File.Exists(thumbnailPath))
                            {
                                System.IO.File.Delete(thumbnailPath);
                            }
                        }

                        // Remove from database
                        await _documentRepository.DeleteAsync(document.DocumentId);
                        cleanedCount++;
                    }
                }

                return Json(new { success = true, message = $"Cleaned up {cleanedCount} files" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }



        [HttpPost]
        public async Task<IActionResult> CleanupUserTempFiles()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Json(new { success = false, error = "User not found" });
                }

                // Find all pending documents uploaded by current user in last 24 hours
                var cutoffTime = DateTime.UtcNow.AddHours(-24);
                var tempDocuments = _documentRepository.GetAllQueryable(d =>
                    d.UploaderId == currentUser.Id &&
                    d.Status == "pending" &&
                    d.CreatedAt < cutoffTime).ToList();

                int cleanedCount = 0;
                foreach (var document in tempDocuments)
                {
                    // Delete main physical file
                    if (!string.IsNullOrEmpty(document.FilePath))
                    {
                        string fullPath = Path.Combine(_environment.WebRootPath, document.FilePath.TrimStart('/'));
                        if (System.IO.File.Exists(fullPath))
                        {
                            System.IO.File.Delete(fullPath);
                        }
                    }

                    // Delete PDF file
                    if (!string.IsNullOrEmpty(document.FilePDFpath))
                    {
                        string pdfPath = Path.Combine(_environment.WebRootPath, document.FilePDFpath.TrimStart('/'));
                        if (System.IO.File.Exists(pdfPath))
                        {
                            System.IO.File.Delete(pdfPath);
                        }
                    }

                    // Delete thumbnail if exists
                    if (!string.IsNullOrEmpty(document.ThumbnailUrl))
                    {
                        string thumbnailPath = Path.Combine(_environment.WebRootPath, document.ThumbnailUrl.TrimStart('/'));
                        if (System.IO.File.Exists(thumbnailPath))
                        {
                            System.IO.File.Delete(thumbnailPath);
                        }
                    }

                    // Remove from database
                    await _documentRepository.DeleteAsync(document.DocumentId);
                    cleanedCount++;
                }

                return Json(new { success = true, message = $"Cleaned up {cleanedCount} old temp files" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }

    //Helper classes
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
        [Range(1, int.MaxValue, ErrorMessage = "Please select a course")]
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