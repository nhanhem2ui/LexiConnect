
using BusinessObjects;

namespace LexiConnect.Models.ViewModels
{
    public class LibraryViewModel
    {
        public List<Document> FavoriteDocuments { get; set; } = new List<Document>();

        public List<Course> FollowedCourses { get; set; } = new List<Course>();


    }
}
