using BusinessObjects;

namespace LexiConnect.Models.ViewModels
{
    public class CourseDetailsViewModel
    {
        public Course Course { get; set; }
        public List<Document> TrendingDocuments { get; set; } = new List<Document>();
        public List<Document> LectureNotes { get; set; } = new List<Document>();
        public List<Document> Quizzes { get; set; } = new List<Document>();
        public List<Document> Assignments { get; set; } = new List<Document>();
        public List<Document> Exams { get; set; } = new List<Document>();
        public List<Document> StudyGuides { get; set; } = new List<Document>();

        // Statistics
        public int TotalDocuments { get; set; }
        public int TotalQuestions { get; set; }
        public int TotalQuizzes { get; set; }
        public int TotalStudents { get; set; }

        // User interaction
        public bool IsFollowing { get; set; }

        // Sorting and filtering
        public string CurrentSort { get; set; } = "rating";
        public string CurrentFilter { get; set; } = "all";
    }
}