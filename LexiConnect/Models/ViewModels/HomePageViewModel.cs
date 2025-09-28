using BusinessObjects;

namespace LexiConnect.Models.ViewModels
{
    public class HomePageViewModel
    {
        public IQueryable<RecentViewed> RecentVieweds { get; set; }

        public IQueryable<Document> TopDocuments { get; set; }

        public Dictionary<int, bool> UserLikedDocuments { get; set; } = new Dictionary<int, bool>();


    }
}
