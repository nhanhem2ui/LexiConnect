using BusinessObjects;
using LexiConnect.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Services;
using System.Security.Claims;

namespace LexiConnect.Controllers
{
    public class CourseController : Controller
    {
        private readonly IGenericService<Course> _courseRepo;
        private readonly IGenericService<Document> _documentRepo;
        private readonly IGenericService<UserFollowCourse> _userFollowCourseRepo;

        public CourseController(IGenericService<Course> courseRepo, IGenericService<Document> documentRepo, IGenericService<UserFollowCourse> userFollowCourseRepo)
        {
            _courseRepo = courseRepo;
            _documentRepo = documentRepo;
            _userFollowCourseRepo = userFollowCourseRepo;
        }

        [HttpGet]
        public async Task<IActionResult> CourseDetails(int id, string sort = "rating")
        {
            // Get course with related data
            var course = await _courseRepo.GetAllQueryable(c => c.CourseId == id)
                .Include(c => c.Major)
                .Include(c => c.Documents)
                    .ThenInclude(d => d.Uploader)
                .FirstOrDefaultAsync();

            if (course == null)
            {
                return NotFound();
            }

            // Get all approved documents for this course
            var allDocuments = await _documentRepo.GetAllQueryable(
                d => d.CourseId == id && d.Status == "approved",
                asNoTracking: true)
                .Include(d => d.Uploader)
                .ToListAsync();

            var trendingDocuments = allDocuments
                .OrderByDescending(d => //weighted score
                    (d.ViewCount * 0.4) +
                    (d.LikeCount * 0.4) +
                    (d.DownloadCount * 0.2) +
                    //Recency boost if less than 30 days
                    (DateTime.UtcNow.Subtract(d.CreatedAt).TotalDays < 30 ? 50 : 0))
                .Take(6)
                .ToList();

            // Group documents by type
            var lectureNotes = allDocuments
                .Where(d => d.DocumentType == "notes")
                .ToList();

            var quizzes = allDocuments
                .Where(d => d.DocumentType == "quiz")
                .ToList();

            var assignments = allDocuments
                .Where(d => d.DocumentType == "assignment")
                .ToList();

            var exams = allDocuments
                .Where(d => d.DocumentType == "exam")
                .ToList();

            var studyGuides = allDocuments
                .Where(d => d.DocumentType == "study_guide")
                .ToList();

            // Apply sorting
            lectureNotes = ApplySorting(lectureNotes, sort);
            quizzes = ApplySorting(quizzes, sort);
            assignments = ApplySorting(assignments, sort);
            exams = ApplySorting(exams, sort);
            studyGuides = ApplySorting(studyGuides, sort);

            // Check if user is following this course
            bool isFollowing = false;
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                // TODO: Check if user follows this course from UserFollowCourse table
                // isFollowing = await _userFollowCourseRepo.ExistsAsync(ufc => ufc.UserId == userId && ufc.CourseId == id);
            }

            // Get statistics
            var totalStudents = await _documentRepo.GetAllQueryable(
                d => d.CourseId == id && d.Status == "approved")
                .Select(d => d.UploaderId)
                .Distinct()
                .CountAsync();

            var viewModel = new CourseDetailsViewModel
            {
                Course = course,
                TrendingDocuments = trendingDocuments,
                LectureNotes = lectureNotes,
                Quizzes = quizzes,
                Assignments = assignments,
                Exams = exams,
                StudyGuides = studyGuides,
                TotalDocuments = allDocuments.Count,
                TotalQuestions = 0, // TODO: implement Questions
                TotalQuizzes = quizzes.Count,
                TotalStudents = totalStudents,
                IsFollowing = isFollowing,
                CurrentSort = sort
            };

            return View(viewModel);
        }

        private List<Document> ApplySorting(List<Document> documents, string sort)
        {
            static double GetRating(Document d)
            {
                if (d.LikeCount == 0) return 0;
                var views = d.ViewCount > 0 ? d.ViewCount : 1;
                return (double)d.LikeCount / views;
            }

            return sort.ToLower() switch
            {
                "date" => documents
                    .OrderByDescending(d => d.CreatedAt)
                    .ToList(),

                "rating" => documents
                    .OrderByDescending(GetRating)
                    .ThenByDescending(d => d.LikeCount)
                    .ToList(),

                "views" => documents
                    .OrderByDescending(d => d.ViewCount)
                    .ToList(),

                "downloads" => documents
                    .OrderByDescending(d => d.DownloadCount)
                    .ToList(),

                _ => documents
                    .OrderByDescending(d => d.LikeCount)
                    .ToList()
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFollowCourse(int courseId)
        {
            if (!User.Identity?.IsAuthenticated == true)
            {
                return Unauthorized();
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var _ = await _courseRepo.GetAsync(c => c.CourseId == courseId) is null
                ? TempData["Error"] = "Course not exists"
                : null;

            // Check if already following
            // If following: remove the follow
            // If not following: add the follow
            var courseFollow = await _userFollowCourseRepo.GetAsync(u => u.UserId.Equals(userId));
            if (courseFollow == null)
            {
                var followed = new UserFollowCourse
                {
                    CourseId = courseId,
                    UserId = userId,
                    FollowedAt = DateTime.UtcNow,
                };
                await _userFollowCourseRepo.AddAsync(followed);

                return Json(new { success = true, isFollowing = true });
            }
            else
            {
                await _userFollowCourseRepo.DeleteAsync(courseFollow.Id);

                return Json(new { success = true, isFollowing = false });
            }
        }

        [HttpGet]
        public async Task<IActionResult> SearchCourseDocuments(int courseId, string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new { documents = new List<object>() });
            }

            var documents = await _documentRepo.GetAllQueryable(
                d => d.CourseId == courseId &&
                     d.Status == "approved" &&
                     (d.Title.Contains(query) ||
                      d.Description.Contains(query)),
                asNoTracking: true)
                .Include(d => d.Uploader)
                .Take(10)
                .Select(d => new
                {
                    d.DocumentId,
                    d.Title,
                    d.Description,
                    d.DocumentType,
                    d.ViewCount,
                    d.LikeCount,
                    d.PageCount,
                    d.ThumbnailUrl
                })
                .ToListAsync();

            return Json(new { documents });
        }

        // NEW: AllCourses action
        [HttpGet]
        public async Task<IActionResult> AllCourses(string search = "", string letter = "All")
        {
            IQueryable<Course> query = _courseRepo.GetAllQueryable(c => c.IsActive, asNoTracking: true)
                .Include(c => c.Major)
                .Include(c => c.Documents);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(c =>
                    c.CourseName.ToLower().Contains(search) ||
                    c.CourseCode.ToLower().Contains(search) ||
                    (c.Description != null && c.Description.ToLower().Contains(search)));
            }

            // Apply letter filter
            if (letter != "All" && !string.IsNullOrEmpty(letter))
            {
                query = query.Where(c => c.CourseName.StartsWith(letter));
            }

            // Get all courses with limit
            var courses = await query
                .OrderBy(c => c.CourseName)
                .Take(30)
                .ToListAsync();

            // Get popular courses (most documents)
            var popularCourses = await _courseRepo.GetAllQueryable(c => c.IsActive, asNoTracking: true)
                .Include(c => c.Major)
                .Include(c => c.Documents)
                .OrderByDescending(c => c.Documents.Count(d => d.Status == "approved"))
                .Take(10)
                .ToListAsync();

            var viewModel = new AllCoursesViewModel
            {
                Courses = courses,
                PopularCourses = popularCourses,
                SearchQuery = search,
                SelectedLetter = letter
            };

            return View(viewModel);
        }

        // NEW: Course search suggestions API
        [HttpGet]
        public async Task<IActionResult> CoursesSearchSuggestions(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                return Json(new List<object>());
            }

            query = query.Trim().ToLower();

            var courses = await _courseRepo.GetAllQueryable(
                c => c.IsActive &&
                    (c.CourseName.ToLower().Contains(query) ||
                     c.CourseCode.ToLower().Contains(query) ||
                     (c.Description != null && c.Description.ToLower().Contains(query))),
                asNoTracking: true)
                .Include(c => c.Major)
                .Include(c => c.Documents)
                .OrderBy(c => c.CourseName)
                .Take(8)
                .Select(c => new
                {
                    courseId = c.CourseId,
                    courseCode = c.CourseCode,
                    courseName = c.CourseName,
                    majorName = c.Major != null ? c.Major.Name : null,
                    semester = c.Semester,
                    documentCount = c.Documents.Count(d => d.Status == "approved"),
                    url = Url.Action("CourseDetails", "Course", new { id = c.CourseId })
                })
                .ToListAsync();

            return Json(courses);
        }
    }
}