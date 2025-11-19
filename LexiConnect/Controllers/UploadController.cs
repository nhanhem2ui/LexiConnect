using BusinessObjects;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using LexiConnect.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Services;
using System.ComponentModel.DataAnnotations;
using Document = BusinessObjects.Document;
using ITextPageSize = iText.Kernel.Geom.PageSize;
using ITextTextAlignment = iText.Layout.Properties.TextAlignment;
using Paragraph = iText.Layout.Element.Paragraph;
// ✅ ALIAS ĐỂ TRÁNH XUNG ĐỘT
using SystemPath = System.IO.Path;
using Table = iText.Layout.Element.Table;
using Text = iText.Layout.Element.Text;


namespace LexiConnect.Controllers
{
    public class UploadController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IGenericService<Document> _documentService;
        private readonly IGenericService<Course> _courseService;
        private readonly IGenericService<University> _universityService;
        private readonly UserManager<Users> _userManager;
        private readonly long _maxFileSize = 10 * 1024 * 1024; // 10MB
        private readonly string[] _allowedExtensions = { ".pdf", ".doc", ".docx" };

        public UploadController(IWebHostEnvironment environment, IGenericService<Document> documentService, UserManager<Users> userManager, IGenericService<University> universityService, IGenericService<Course> courseService)
        {
            _environment = environment;
            _documentService = documentService;
            _userManager = userManager;
            _courseService = courseService;
            _universityService = universityService;
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
                var currentUser = await _userManager.GetUserAsync(User);
                if (!User.Identity.IsAuthenticated)
                {
                    TempData["Error"] = "You must be logged in to upload documents.";
                    return RedirectToAction("Signin", "Auth");
                }

                if (files == null || !files.Any())
                {
                    TempData["Error"] = "Please select at least one file to upload.";
                    return RedirectToAction("Index");
                }

                string uploadPath = SystemPath.Combine(_environment.WebRootPath, "uploads", "documents");
                string thumbnailPath = SystemPath.Combine(_environment.WebRootPath, "uploads", "thumbnails");

                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }
                if (!Directory.Exists(thumbnailPath))
                {
                    Directory.CreateDirectory(thumbnailPath);
                }

                foreach (var file in files)
                {
                    var result = await ProcessSingleFile(file, uploadPath, thumbnailPath, currentUser.Id);
                    uploadResults.Add(result);
                }

                var successfulUploads = uploadResults.Where(r => r.Success).ToList();
                var failedUploads = uploadResults.Where(r => !r.Success).ToList();

                if (successfulUploads.Any())
                {
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
                    TempData["Error"] = "No files were successfully uploaded. Please try again.";
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
                var document = await _documentService.GetAsync(d => d.DocumentId == id);
                if (document == null)
                    return Json(new { success = false, error = "Document not found" });

                if (!string.IsNullOrEmpty(document.FilePath))
                {
                    string fullPath = SystemPath.Combine(_environment.WebRootPath, document.FilePath.TrimStart('/'));
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                        Console.WriteLine($"Main file deleted: {fullPath}");
                    }
                }

                if (!string.IsNullOrEmpty(document.FilePDFpath))
                {
                    string pdfPath = SystemPath.Combine(_environment.WebRootPath, document.FilePDFpath.TrimStart('/'));
                    if (System.IO.File.Exists(pdfPath))
                    {
                        System.IO.File.Delete(pdfPath);
                        Console.WriteLine($"PDF file deleted: {pdfPath}");
                    }
                }

                if (!string.IsNullOrEmpty(document.ThumbnailUrl))
                {
                    string thumbnailPath = SystemPath.Combine(_environment.WebRootPath, document.ThumbnailUrl.TrimStart('/'));
                    if (System.IO.File.Exists(thumbnailPath))
                    {
                        System.IO.File.Delete(thumbnailPath);
                        Console.WriteLine($"Thumbnail deleted: {thumbnailPath}");
                    }
                }

                await _documentService.DeleteAsync(document.DocumentId);

                if (TempData["DocumentIds"] != null)
                {
                    var documentIdsString = TempData["DocumentIds"].ToString();
                    var documentIds = documentIdsString.Split(',').Select(int.Parse).ToList();
                    documentIds.Remove(id);

                    if (documentIds.Any())
                    {
                        TempData["DocumentIds"] = string.Join(",", documentIds);
                        TempData.Keep("DocumentIds");
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
                var documentIdsString = TempData["DocumentIds"].ToString();
                var documentIds = documentIdsString.Split(',').Where(id => !string.IsNullOrWhiteSpace(id)).Select(int.Parse).ToList();

                if (!documentIds.Any())
                {
                    TempData["Info"] = "No valid documents found. Please upload some documents first.";
                    return RedirectToAction("Index");
                }

                var documents = new List<SingleDocumentModel>();
                foreach (var docId in documentIds)
                {
                    var document = await _documentService.GetAsync(d => d.DocumentId == docId);
                    if (document != null)
                    {
                        documents.Add(new SingleDocumentModel
                        {
                            DocumentId = document.DocumentId,
                            FileName = SystemPath.GetFileName(document.Title),
                            Title = document.Title,
                            DocumentType = document.DocumentType,
                            Description = document.Description,
                            PublicYear = DateTime.Now
                        });
                    }
                }

                if (!documents.Any())
                {
                    TempData["Warning"] = "No documents were found in the database. They may have been deleted.";
                    return RedirectToAction("Index");
                }

                var courses = _courseService.GetAllQueryable().Select(c => new SelectListItem
                {
                    Value = c.CourseId.ToString(),
                    Text = $"{c.CourseCode} - {c.CourseName}"
                }).ToList();

                courses.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "Select a course",
                    Selected = true
                });

                var model = new DocumentDetailsViewModel { Documents = documents, Courses = courses };

                ViewBag.SuccessfulUploads = TempData["SuccessfulUploads"];
                ViewBag.UploadedFiles = TempData["UploadedFiles"];
                TempData.Keep("DocumentIds");

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading document details: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // Add this API endpoint for university search
        [HttpGet]
        public async Task<IActionResult> SearchUniversities(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    // Return top 50 universities if no search query
                    var allUniversities = _universityService.GetAllQueryable()
                        .OrderBy(u => u.Name)
                        .Take(50)
                        .Select(u => new
                        {
                            id = u.Id,
                            name = u.Name,
                            shortName = u.ShortName,
                            city = u.City,
                            country = u.Country.CountryName,
                            isVerified = u.IsVerified
                        })
                        .ToList();

                    return Json(allUniversities);
                }

                var searchQuery = searchTerm.ToLower().Trim();

                var universities = _universityService.GetAllQueryable(u =>
                    u.Name.ToLower().Contains(searchQuery) ||
                    (u.ShortName != null && u.ShortName.ToLower().Contains(searchQuery)) ||
                    u.City.ToLower().Contains(searchQuery)
                )
                .OrderByDescending(u => u.IsVerified)
                .ThenBy(u => u.Name)
                .Take(20)
                .Select(u => new
                {
                    id = u.Id,
                    name = u.Name,
                    shortName = u.ShortName,
                    city = u.City,
                    country = u.Country.CountryName,
                    isVerified = u.IsVerified
                })
                .ToList();

                return Json(universities);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // API endpoint để get university by ID
        [HttpGet]
        public async Task<IActionResult> GetUniversity(int id)
        {
            try
            {
                var university = await _universityService.GetAsync(u => u.Id == id);
                if (university == null)
                    return Json(new { error = "University not found" });

                return Json(new
                {
                    id = university.Id,
                    name = university.Name,
                    shortName = university.ShortName,
                    city = university.City,
                    logoUrl = university.LogoUrl
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDetails(DocumentDetailsViewModel model)
        {
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

                    foreach (var documentModel in model.Documents)
                    {
                        var document = await _documentService.GetAsync(d => d.DocumentId == documentModel.DocumentId);
                        if (document != null)
                        {
                            document.Title = documentModel.Title;
                            document.Description = documentModel.Description;
                            document.DocumentType = documentModel.DocumentType;
                            document.CourseId = documentModel.CourseId;
                            document.UniversityId = documentModel.UniversityId; // Save university
                            document.UpdatedAt = DateTime.UtcNow;
                            document.PointsAwarded = GetPointsForDocumentType(document.DocumentType);
                            document.PointsToDownload = GetDownloadPointsForDocumentType(document.DocumentType);
                            document.Status = "pending";

                            await _documentService.UpdateAsync(document);
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

        public IActionResult Complete()
        {
            return View();
        }

        private async Task<UploadResult> ProcessSingleFile(IFormFile file, string uploadPath, string thumbnailPath, string uploaderId)
        {
            var result = new UploadResult { FileName = file.FileName };

            try
            {
                var validationResult = ValidateFile(file);
                if (!validationResult.IsValid)
                {
                    result.Success = false;
                    result.ErrorMessage = validationResult.ErrorMessage;
                    return result;
                }

                string fileExtension = SystemPath.GetExtension(file.FileName).ToLowerInvariant();
                string uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                string filePath = SystemPath.Combine(uploadPath, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                string pdfPath = await GeneratePDFPath(filePath, fileExtension, uniqueFileName);

                var document = new Document
                {
                    Title = SystemPath.GetFileNameWithoutExtension(file.FileName),
                    Description = $"Uploaded document: {file.FileName}",
                    DocumentType = DetermineDocumentType(file.FileName),
                    FilePath = $"/uploads/documents/{uniqueFileName}",
                    FileSize = file.Length,
                    FileType = fileExtension.TrimStart('.'),
                    UploaderId = uploaderId,
                    CourseId = 1,
                    Status = "pending",
                    LanguageCode = "en",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    PageCount = await EstimatePageCount(filePath, fileExtension),
                    PointsAwarded = 0,
                    PointsToDownload = 0,
                    IsPremiumOnly = false,
                    IsAiGenerated = false,
                    FilePDFpath = pdfPath
                };

                if (fileExtension == ".pdf")
                {
                    document.ThumbnailUrl = await GeneratePdfThumbnail(filePath, thumbnailPath, uniqueFileName);
                }

                // Validate CourseId exists before saving
                var courseExists = await _courseService.ExistsAsync(c => c.CourseId == document.CourseId);
                if (!courseExists)
                {
                    // Try to get the first available course
                    var firstCourse = await _courseService.GetAllQueryable().FirstOrDefaultAsync();
                    if (firstCourse != null)
                    {
                        document.CourseId = firstCourse.CourseId;
                        Console.WriteLine($"Warning: CourseId 1 not found. Using CourseId {firstCourse.CourseId} instead.");
                    }
                    else
                    {
                        result.Success = false;
                        result.ErrorMessage = "No courses available in the database. Please add a course first.";
                        return result;
                    }
                }

                bool saved = await _documentService.AddAsync(document);
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

                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    if (!string.IsNullOrEmpty(pdfPath))
                    {
                        string fullPdfPath = SystemPath.Combine(_environment.WebRootPath, pdfPath.TrimStart('/'));
                        if (System.IO.File.Exists(fullPdfPath))
                        {
                            System.IO.File.Delete(fullPdfPath);
                        }
                    }
                }
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                result.Success = false;
                var errorDetails = dbEx.InnerException?.Message ?? dbEx.Message;
                
                // Check for specific database errors
                if (errorDetails.Contains("FOREIGN KEY constraint") || errorDetails.Contains("The INSERT statement conflicted"))
                {
                    if (errorDetails.Contains("CourseId") || errorDetails.Contains("Course"))
                    {
                        result.ErrorMessage = $"Database error: CourseId không tồn tại trong database. Vui lòng kiểm tra lại CourseId trong database Azure SQL.";
                    }
                    else if (errorDetails.Contains("UploaderId") || errorDetails.Contains("AspNetUsers"))
                    {
                        result.ErrorMessage = $"Database error: User không tồn tại trong database. Vui lòng đăng nhập lại.";
                    }
                    else
                    {
                        result.ErrorMessage = $"Database constraint error: {errorDetails}";
                    }
                }
                else if (errorDetails.Contains("timeout") || errorDetails.Contains("Timeout"))
                {
                    result.ErrorMessage = $"Database timeout: Kết nối tới Azure SQL Database quá chậm. Vui lòng thử lại sau.";
                }
                else if (errorDetails.Contains("Cannot open database") || errorDetails.Contains("Login failed"))
                {
                    result.ErrorMessage = $"Database connection error: Không thể kết nối tới Azure SQL Database. Vui lòng kiểm tra connection string.";
                }
                else
                {
                    result.ErrorMessage = $"Database error khi upload {file.FileName}: {errorDetails}";
                }
                
                Console.WriteLine($"Database error details: {dbEx}");
                if (dbEx.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {dbEx.InnerException}");
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Error uploading {file.FileName}: {ex.Message}";
                Console.WriteLine($"Exception details: {ex}");
            }

            return result;
        }

        // ✅ HELPER METHOD ĐÃ SỬA - CHỈ 2 THAM SỐ
        private async Task<string> GeneratePDFPath(string originalFilePath, string fileExtension, string uniqueFileName)
        {
            try
            {
                string pdfPath = SystemPath.Combine(_environment.WebRootPath, "uploads", "pdf");
                if (!Directory.Exists(pdfPath))
                {
                    Directory.CreateDirectory(pdfPath);
                }

                string pdfFileName = $"{SystemPath.GetFileNameWithoutExtension(uniqueFileName)}.pdf";
                string fullPdfPath = SystemPath.Combine(pdfPath, pdfFileName);

                if (fileExtension == ".pdf")
                {
                    System.IO.File.Copy(originalFilePath, fullPdfPath, true);
                }
                else if (fileExtension == ".docx")
                {
                    await ConvertDocxToPdf(originalFilePath, fullPdfPath);
                }
                else if (fileExtension == ".doc")
                {
                    await CreateDocConversionPdf(originalFilePath, fullPdfPath);
                }

                return $"/uploads/pdf/{pdfFileName}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating PDF path: {ex.Message}");
                return string.Empty;
            }
        }

        // ✅ CONVERT DOCX TO PDF - ĐÃ SỬA (CHỈ 2 THAM SỐ)
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
                        var document = new iText.Layout.Document(pdf, ITextPageSize.A4);
                        document.SetMargins(50, 50, 50, 50);

                        PdfFont vietnameseFont = GetVietnameseFont();
                        PdfFont boldFont = GetBoldVietnameseFont();

                        document.SetFont(vietnameseFont);
                        document.SetFontSize(12);

                        try
                        {
                            foreach (var element in body.Elements())
                            {
                                if (element is DocumentFormat.OpenXml.Wordprocessing.Paragraph paragraph)
                                {
                                    ProcessParagraph(document, paragraph, vietnameseFont, boldFont);
                                }
                                else if (element is DocumentFormat.OpenXml.Wordprocessing.Table table)
                                {
                                    ProcessTable(document, table, vietnameseFont, boldFont);
                                }
                            }

                            if (document.GetRenderer().GetChildRenderers().Count == 0)
                            {
                                document.Add(new Paragraph("Nội dung tài liệu không thể trích xuất.")
                                    .SetFont(vietnameseFont)
                                    .SetFontSize(12)
                                    .SetFontColor(ColorConstants.GRAY));
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing document content: {ex.Message}");
                            document.Add(new Paragraph($"Lỗi xử lý nội dung: {ex.Message}")
                                .SetFont(vietnameseFont)
                                .SetFontColor(ColorConstants.RED));
                        }

                        document.Close();
                    }
                }
            });
        }

        // ✅ HÀM LẤY FONT TIẾNG VIỆT
        private PdfFont GetVietnameseFont()
        {
            try
            {
                string arialPath = SystemPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
                if (System.IO.File.Exists(arialPath))
                {
                    return PdfFontFactory.CreateFont(arialPath, "Identity-H", PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
                }

                string timesPath = SystemPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "times.ttf");
                if (System.IO.File.Exists(timesPath))
                {
                    return PdfFontFactory.CreateFont(timesPath, "Identity-H", PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
                }

                string customFontPath = SystemPath.Combine(_environment.WebRootPath, "fonts", "NotoSans-Regular.ttf");
                if (System.IO.File.Exists(customFontPath))
                {
                    return PdfFontFactory.CreateFont(customFontPath, "Identity-H", PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
                }

                return PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading Vietnamese font: {ex.Message}");
                return PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            }
        }

        private PdfFont GetBoldVietnameseFont()
        {
            try
            {
                string arialBoldPath = SystemPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arialbd.ttf");
                if (System.IO.File.Exists(arialBoldPath))
                {
                    return PdfFontFactory.CreateFont(arialBoldPath, "Identity-H", PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
                }

                string timesBoldPath = SystemPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "timesbd.ttf");
                if (System.IO.File.Exists(timesBoldPath))
                {
                    return PdfFontFactory.CreateFont(timesBoldPath, "Identity-H", PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
                }

                return PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            }
            catch
            {
                return PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            }
        }

        // ✅ XỬ LÝ PARAGRAPH
        private void ProcessParagraph(iText.Layout.Document document, DocumentFormat.OpenXml.Wordprocessing.Paragraph paragraph, PdfFont normalFont, PdfFont boldFont)
        {
            var pdfParagraph = new Paragraph();
            bool hasContent = false;

            foreach (var run in paragraph.Elements<DocumentFormat.OpenXml.Wordprocessing.Run>())
            {
                string text = run.InnerText;
                if (!string.IsNullOrWhiteSpace(text))
                {
                    hasContent = true;

                    bool isBold = run.RunProperties?.Bold != null;
                    bool isItalic = run.RunProperties?.Italic != null;
                    bool isUnderline = run.RunProperties?.Underline != null;

                    var textElement = new Text(text);
                    textElement.SetFont(isBold ? boldFont : normalFont);

                    if (isItalic)
                    {
                        textElement.SetItalic();
                    }
                    if (isUnderline)
                    {
                        textElement.SetUnderline();
                    }

                    var color = run.RunProperties?.Color?.Val?.Value;
                    if (!string.IsNullOrEmpty(color) && color.Length == 6)
                    {
                        try
                        {
                            int r = Convert.ToInt32(color.Substring(0, 2), 16);
                            int g = Convert.ToInt32(color.Substring(2, 2), 16);
                            int b = Convert.ToInt32(color.Substring(4, 2), 16);
                            textElement.SetFontColor(new DeviceRgb(r, g, b));
                        }
                        catch { }
                    }

                    pdfParagraph.Add(textElement);
                }
            }

            if (hasContent)
            {
                var alignment = paragraph.ParagraphProperties?.Justification?.Val?.Value;
                if (alignment != null)
                {
                    switch (alignment.ToString())
                    {
                        case "center":
                            pdfParagraph.SetTextAlignment(ITextTextAlignment.CENTER);
                            break;
                        case "right":
                            pdfParagraph.SetTextAlignment(ITextTextAlignment.RIGHT);
                            break;
                        case "both":
                            pdfParagraph.SetTextAlignment(ITextTextAlignment.JUSTIFIED);
                            break;
                    }
                }

                pdfParagraph.SetMarginBottom(10);
                document.Add(pdfParagraph);
            }
            else
            {
                document.Add(new Paragraph("\u00A0").SetMarginBottom(5));
            }
        }

        // ✅ XỬ LÝ TABLE
        private void ProcessTable(iText.Layout.Document document, DocumentFormat.OpenXml.Wordprocessing.Table table, PdfFont normalFont, PdfFont boldFont)
        {
            try
            {
                var rows = table.Elements<DocumentFormat.OpenXml.Wordprocessing.TableRow>().ToList();
                if (!rows.Any()) return;

                int maxColumns = rows.Max(row => row.Elements<DocumentFormat.OpenXml.Wordprocessing.TableCell>().Count());

                var pdfTable = new Table(maxColumns);
                pdfTable.SetWidth(UnitValue.CreatePercentValue(100));
                pdfTable.SetMarginBottom(10);

                foreach (var row in rows)
                {
                    var cells = row.Elements<DocumentFormat.OpenXml.Wordprocessing.TableCell>().ToList();

                    foreach (var cell in cells)
                    {
                        var cellText = cell.InnerText;
                        var cellParagraph = new Paragraph(cellText)
                            .SetFont(normalFont)
                            .SetFontSize(10)
                            .SetMargin(5);

                        var pdfCell = new Cell()
                            .Add(cellParagraph)
                            .SetBorder(new SolidBorder(ColorConstants.GRAY, 1))
                            .SetPadding(5);

                        pdfTable.AddCell(pdfCell);
                    }

                    int cellsToAdd = maxColumns - cells.Count;
                    for (int i = 0; i < cellsToAdd; i++)
                    {
                        pdfTable.AddCell(new Cell()
                            .Add(new Paragraph(""))
                            .SetBorder(new SolidBorder(ColorConstants.GRAY, 1)));
                    }
                }

                document.Add(pdfTable);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing table: {ex.Message}");
                document.Add(new Paragraph("[Bảng không thể hiển thị]")
                    .SetFont(normalFont)
                    .SetFontColor(ColorConstants.RED));
            }
        }

        private async Task CreateDocConversionPdf(string docPath, string pdfPath)
        {
            await Task.Run(() =>
            {
                using (var writer = new PdfWriter(pdfPath))
                using (var pdf = new PdfDocument(writer))
                {
                    var document = new iText.Layout.Document(pdf);
                    var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                    document.Add(new Paragraph("Document Conversion Notice")
                        .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                        .SetFontSize(16)
                        .SetTextAlignment(ITextTextAlignment.CENTER));

                    document.Add(new Paragraph("\n"));

                    document.Add(new Paragraph($"Original file: {SystemPath.GetFileName(docPath)}")
                        .SetFont(font)
                        .SetFontSize(12));

                    document.Add(new Paragraph("File type: Microsoft Word Document (.doc)")
                        .SetFont(font)
                        .SetFontSize(12));

                    document.Add(new Paragraph($"Converted on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                        .SetFont(font)
                        .SetFontSize(12));

                    document.Add(new Paragraph("\n"));

                    document.Add(new Paragraph("Note: .DOC files require Microsoft Office or LibreOffice for full content extraction. This is a placeholder PDF. For complete conversion, consider upgrading to .DOCX format or implementing Office automation.")
                        .SetFont(font)
                        .SetFontSize(10)
                        .SetFontColor(ColorConstants.GRAY));

                    document.Close();
                }
            });
        }

        private FileValidationResult ValidateFile(IFormFile file)
        {
            if (file.Length == 0)
            {
                return new FileValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"{file.FileName} is empty."
                };
            }

            if (file.Length > _maxFileSize)
            {
                return new FileValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"{file.FileName} exceeds maximum size of {_maxFileSize / 1024 / 1024}MB."
                };
            }

            string fileExtension = SystemPath.GetExtension(file.FileName).ToLowerInvariant();
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
                    var document = await _documentService.GetAsync(d => d.DocumentId == id);
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
                        await _documentService.DeleteAsync(document.DocumentId);
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
                var tempDocuments = _documentService.GetAllQueryable(d =>
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
                    await _documentService.DeleteAsync(document.DocumentId);
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
        public int? UniversityId { get; set; }

        // Giữ lại để hiển thị
        public string UniversityName { get; set; } = string.Empty;

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