using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LexiConnect.Models.ViewModels;
using Repositories;
using BusinessObjects;

namespace LexiConnect.Controllers
{
    public class UniversityController : Controller
    {
        private readonly IGenericRepository<University> _universityRepo;
        private readonly IGenericRepository<Course> _courseRepo;
        private readonly IGenericRepository<Document> _documentRepo;

        public UniversityController(
            IGenericRepository<University> universityRepo,
            IGenericRepository<Course> courseRepo,
            IGenericRepository<Document> documentRepo)
        {
            _universityRepo = universityRepo;
            _courseRepo = courseRepo;
            _documentRepo = documentRepo;
        }

        public async Task<IActionResult> Details(int id, string sort = "name", string search = "", string letter = "All")
        {
            var university = await _universityRepo.GetAllQueryable(u => u.Id == id)
                .Include(u => u.Country)
                .FirstOrDefaultAsync();

            if (university == null)
                return NotFound();

            // Get all courses for this university with stats
            var coursesQuery = _courseRepo.GetAllQueryable(
                c => c.IsActive && c.Major.UniversityId == id,
                asNoTracking: true)
                .Include(c => c.Major)
                .Include(c => c.Documents)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                coursesQuery = coursesQuery.Where(c =>
                    c.CourseName.Contains(search) ||
                    c.CourseCode.Contains(search) ||
                    c.Major.Name.Contains(search));
            }

            // Apply letter filter
            if (letter != "All" && !string.IsNullOrEmpty(letter))
            {
                coursesQuery = coursesQuery.Where(c => c.CourseName.StartsWith(letter));
            }

            var courses = await coursesQuery.Select(c => new CourseWithStats
            {
                CourseId = c.CourseId,
                CourseCode = c.CourseCode,
                CourseName = c.CourseName,
                MajorName = c.Major.Name,
                DocumentCount = c.Documents.Count(d => d.Status == "approved"),
                StudentCount = 0, // Would need a separate student enrollment table
                LastUpdated = c.Documents
                    .OrderByDescending(d => d.CreatedAt)
                    .Select(d => (DateTime?)d.CreatedAt)
                    .FirstOrDefault(),
                Semester = c.Semester,
                AcademicYear = c.AcademicYear
            }).ToListAsync();

            // Apply sorting
            courses = sort switch
            {
                "popular" => courses.OrderByDescending(c => c.DocumentCount).ToList(),
                "recent" => courses.OrderByDescending(c => c.LastUpdated ?? DateTime.MinValue).ToList(),
                _ => courses.OrderBy(c => c.CourseName).ToList()
            };

            // Get popular documents (top 10 by views + likes)
            var popularDocs = await _documentRepo.GetAllQueryable(
                d => d.Status == "approved" && d.Course.Major.UniversityId == id,
                asNoTracking: true)
                .Include(d => d.Course)
                .Include(d => d.Uploader)
                .OrderByDescending(d => d.ViewCount + d.LikeCount)
                .Take(10)
                .Select(d => new DocumentSummary
                {
                    DocumentId = d.DocumentId,
                    Title = d.Title,
                    DocumentType = d.DocumentType,
                    ThumbnailUrl = d.ThumbnailUrl,
                    ViewCount = d.ViewCount,
                    LikeCount = d.LikeCount,
                    PageCount = d.PageCount,
                    CreatedAt = d.CreatedAt,
                    CourseName = d.Course.CourseName,
                    UploaderName = d.Uploader.UserName ?? "Unknown"
                })
                .ToListAsync();

            // Get recent documents (top 10 by creation date)
            var recentDocs = await _documentRepo.GetAllQueryable(
                d => d.Status == "approved" && d.Course.Major.UniversityId == id,
                asNoTracking: true)
                .Include(d => d.Course)
                .Include(d => d.Uploader)
                .OrderByDescending(d => d.CreatedAt)
                .Take(10)
                .Select(d => new DocumentSummary
                {
                    DocumentId = d.DocumentId,
                    Title = d.Title,
                    DocumentType = d.DocumentType,
                    ThumbnailUrl = d.ThumbnailUrl,
                    ViewCount = d.ViewCount,
                    LikeCount = d.LikeCount,
                    PageCount = d.PageCount,
                    CreatedAt = d.CreatedAt,
                    CourseName = d.Course.CourseName,
                    UploaderName = d.Uploader.UserName ?? "Unknown"
                })
                .ToListAsync();

            // Get other universities in the same country
            var otherUniversities = await _universityRepo.GetAllQueryable(
                u => u.Id != id && u.CountryId == university.CountryId && u.IsVerified,
                asNoTracking: true)
                .Take(6)
                .Select(u => new UniversitySummary
                {
                    Id = u.Id,
                    Name = u.Name,
                    ShortName = u.ShortName,
                    City = u.City,
                    LogoUrl = u.LogoUrl,
                    CourseCount = 0, // Would need to count
                    DocumentCount = 0 // Would need to count
                })
                .ToListAsync();

            // Get content categories (document types count)
            var contentCategories = await _documentRepo.GetAllQueryable(
                d => d.Status == "approved" && d.Course.Major.UniversityId == id)
                .GroupBy(d => d.DocumentType)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Type, x => x.Count);

            // Build view model
            var viewModel = new UniversityDetailsViewModel
            {
                University = university,
                TotalCourses = courses.Count,
                TotalDocuments = contentCategories.Values.Sum(),
                TotalStudents = 12500, // This would come from actual data
                Courses = courses,
                CurrentSort = sort,
                SearchQuery = search,
                SelectedLetter = letter,
                PopularDocuments = popularDocs,
                RecentDocuments = recentDocs,
                OtherUniversities = otherUniversities,
                ContentCategories = contentCategories
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> AllUniversities(string search = "", string letter = "All")
        {
            var query = _universityRepo.GetAllQueryable(u => u.IsVerified).Include(u => u.Country).AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    u.Name.Contains(search) ||
                    u.ShortName.Contains(search) ||
                    u.City.Contains(search));
            }

            // Apply letter filter
            if (letter != "All" && !string.IsNullOrEmpty(letter))
            {
                query = query.Where(u => u.Name.StartsWith(letter));
            }

            // Order by name and take max 30
            var universities = await query
                .OrderBy(u => u.Name)
                .Take(30)
                .ToListAsync();

            // Get popular universities (top 10 by document count)
            var popularUniversities = await _universityRepo.GetAllQueryable(
                u => u.IsVerified,
                asNoTracking: true)
                .Include(u => u.Country)
                .OrderBy(u => u.Name)
                .Take(10)
                .ToListAsync();

            var viewModel = new UniversitiesViewModel
            {
                Universities = universities,
                PopularUniversities = popularUniversities,
                SearchQuery = search,
                SelectedLetter = letter
            };

            return View(viewModel);
        }
    }
}