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
                var documents = await _documentRepository.GetAllQueryable()
                    .Include(d => d.Course)
                        .ThenInclude(c => c.Major)
                            .ThenInclude(m => m.University)
                    .Where(d => d.Title.Contains(query) ||
                               d.Description.Contains(query) ||
                               d.Course.CourseName.Contains(query) ||
                               d.Course.CourseCode.Contains(query))
                    .OrderByDescending(d => d.UpdatedAt)
                    .Take(5)
                    .ToListAsync();
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
                var courses = await _courseRepository.GetAllQueryable()
                    .Include(c => c.Major)
                        .ThenInclude(m => m.University)
                    .Where(c => c.CourseName.Contains(query) ||
                               c.CourseCode.Contains(query) ||
                               c.Description.Contains(query))
                    .Take(3)
                    .ToListAsync();

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
                var universities = await _universityRepository.GetAllQueryable()
                .Where(u => u.Name.Contains(query) ||
                           u.ShortName.Contains(query))
                .Take(2)
                .ToListAsync();
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
            var documents = await _documentRepository.GetAllQueryable()
                    .Include(d => d.Course)
                        .ThenInclude(c => c.Major)
                            .ThenInclude(m => m.University)
                    .Where(d => d.Title.Contains(query) ||
                               d.Description.Contains(query) ||
                               d.Course.CourseName.Contains(query) ||
                               d.Course.CourseCode.Contains(query))
                    .OrderByDescending(d => d.UpdatedAt)
                    .Take(5)
                    .ToListAsync();
            var courses = await _courseRepository.GetAllQueryable()
                    .Include(c => c.Major)
                        .ThenInclude(m => m.University)
                    .Where(c => c.CourseName.Contains(query) ||
                               c.CourseCode.Contains(query) ||
                               c.Description.Contains(query))
                    .Take(3)
                    .ToListAsync();
            var universities = await _universityRepository.GetAllQueryable()
                .Where(u => u.Name.Contains(query) ||
                           u.ShortName.Contains(query))
                .Take(5)
                .ToListAsync();

            var model = new SearchResultsViewModel
            {
                Query = query,
                Documents = documents,
                Courses = courses,
                Universities = universities,
                CurrentPage = page,
                PageSize = pageSize
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> UniversitiesSearchSuggestions(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                return Json(new List<object>());
            }

            var universities = await _universityRepository.GetAllQueryable(
                u => u.IsVerified &&
                    (u.Name.Contains(query) ||
                     u.ShortName.Contains(query) ||
                     u.City.Contains(query)),
                asNoTracking: true)
                .Include(u => u.Country)
                .OrderBy(u => u.Name)
                .Take(10)
                .Select(u => new
                {
                    name = u.Name,
                    shortName = u.ShortName,
                    city = u.City,
                    country = u.Country.CountryName,
                    logoUrl = u.LogoUrl,
                    url = Url.Action("Details", "University", new { id = u.Id })
                })
                .ToListAsync();

            return Json(universities);
        }
    }
}
