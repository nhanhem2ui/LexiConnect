using BusinessObjects;
using LexiConnect.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories;

namespace LexiConnect.Controllers
{
    public class SearchController : Controller
    {
        private readonly IGenericRepository<Document> _documentRepository;
        private readonly IGenericRepository<Course> _courseRepository;
        private readonly IGenericRepository<University> _universityRepository;

        public SearchController(
            IGenericRepository<Document> documentRepository,
            IGenericRepository<Course> courseRepository,
            IGenericRepository<University> universityRepository)
        {
            _documentRepository = documentRepository;
            _courseRepository = courseRepository;
            _universityRepository = universityRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Suggestions(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                return Json(new List<object>());
            }

            var suggestions = new List<object>();

            try
            {
                // Search in documents
                var documents = await SearchDocumentsAsync(query, 5);
                foreach (var doc in documents)
                {
                    suggestions.Add(new
                    {
                        title = doc.Title,
                        type = "Document",
                        university = doc.Course?.Major?.University?.Name ?? "Unknown",
                        url = Url.Action("Details", "Document", new { id = doc.DocumentId })
                    });
                }

                // Search in courses
                var courses = await SearchCoursesAsync(query, 3);

                foreach (var course in courses)
                {
                    suggestions.Add(new
                    {
                        title = $"{course.CourseName} ({course.CourseCode})",
                        type = "Course",
                        university = course.Major?.University?.Name ?? "Unknown",
                        url = Url.Action("Details", "Course", new { id = course.CourseId })
                    });
                }

                // Search in universities
                var universities = await SearchUniversitiesAsync(query, 2);
                foreach (var uni in universities)
                {
                    suggestions.Add(new
                    {
                        title = $"{uni.Name} ({uni.ShortName})",
                        type = "University",
                        university = uni.Name,
                        url = Url.Action("UniversityDetails", "University", new { id = uni.Id })
                    });
                }

                // Limit total suggestions
                suggestions = suggestions.Take(10).ToList();
            }
            catch (Exception)
            {
                return Json(new List<object>());
            }

            return Json(suggestions);
        }

        [HttpGet]
        public async Task<IActionResult> Results(string query, int page = 1, int pageSize = 20)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return RedirectToAction("Introduction", "Home");
            }

            var model = new SearchResultsViewModel
            {
                Query = query,
                Documents = await SearchDocumentsAsync(query, pageSize, page),
                Courses = await SearchCoursesAsync(query, 10),
                Universities = await SearchUniversitiesAsync(query, 5),
                CurrentPage = page,
                PageSize = pageSize
            };

            return View(model);
        }
        public async Task<IEnumerable<Document>> SearchDocumentsAsync(string query, int limit, int page = 1)
        {
            return await _documentRepository.GetAllQueryable()
                .Include(d => d.Course)
                    .ThenInclude(c => c.Major)
                        .ThenInclude(m => m.University)
                .Where(d => d.Title.Contains(query) ||
                           d.Description.Contains(query) ||
                           d.Course.CourseName.Contains(query) ||
                           d.Course.CourseCode.Contains(query))
                .OrderByDescending(d => d.UpdatedAt)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();
        }
        public async Task<IEnumerable<Course>> SearchCoursesAsync(string query, int limit)
        {
            return await _courseRepository.GetAllQueryable()
                    .Include(c => c.Major)
                        .ThenInclude(m => m.University)
                    .Where(c => c.CourseName.Contains(query) ||
                               c.CourseCode.Contains(query) ||
                               c.Description.Contains(query))
                    .Take(limit)
                    .ToListAsync();
        }
        public async Task<IEnumerable<University>> SearchUniversitiesAsync(string query, int limit)
        {
            return await _universityRepository.GetAllQueryable()
                .Where(u => u.Name.Contains(query) ||
                           u.ShortName.Contains(query))
                .Take(limit)
                .ToListAsync();
        }
    }
}
