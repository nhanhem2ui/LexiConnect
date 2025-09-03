using BusinessObjects;

namespace LexiConnect.Models.ViewModels
{
    public class IntroductionViewModel
    {
        public IQueryable<University> Universities { get; set; }
        public IQueryable<Course> Courses { get; set; }
    }
}
