using BusinessObjects;

namespace LexiConnect.Models.ViewModels
{
    public class UniversityDetailsViewModel
    {
        public University University { get; set; } = null!;

        // Statistics
        public int TotalCourses { get; set; }
        public int TotalDocuments { get; set; }
        public int TotalStudents { get; set; }

        // Courses with sorting
        public List<CourseWithStats> Courses { get; set; } = new();
        public string CurrentSort { get; set; } = "name"; // name, popular, recent
        public string SearchQuery { get; set; } = string.Empty;
        public string SelectedLetter { get; set; } = "All";

        // Documents
        public List<DocumentSummary> PopularDocuments { get; set; } = new();
        public List<DocumentSummary> RecentDocuments { get; set; } = new();

        // Related universities
        public List<UniversitySummary> OtherUniversities { get; set; } = new();

        // Content categories
        public Dictionary<string, int> ContentCategories { get; set; } = new();
    }

    public class CourseWithStats
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string MajorName { get; set; } = string.Empty;
        public int DocumentCount { get; set; }
        public int StudentCount { get; set; }
        public DateTime? LastUpdated { get; set; }
        public byte? Semester { get; set; }
        public int? AcademicYear { get; set; }
    }

    public class DocumentSummary
    {
        public int DocumentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
        public int? PageCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string UploaderName { get; set; } = string.Empty;
    }

    public class UniversitySummary
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ShortName { get; set; }
        public string City { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public int CourseCount { get; set; }
        public int DocumentCount { get; set; }
    }
}