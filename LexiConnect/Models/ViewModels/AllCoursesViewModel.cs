namespace LexiConnect.Models.ViewModels
{
    public class AllCoursesViewModel
    {
        public List<Course> Courses { get; set; } = new List<Course>();
        public List<Course> PopularCourses { get; set; } = new List<Course>();
        public string SearchQuery { get; set; } = string.Empty;
        public string SelectedLetter { get; set; } = "All";
    }
}
