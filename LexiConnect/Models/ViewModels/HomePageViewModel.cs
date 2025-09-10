using BusinessObjects;

namespace LexiConnect.Models.ViewModels
{
    public class HomePageViewModel
    {
        public IQueryable<RecentViewed> RecentVieweds { get; set; }

        public IQueryable<Document> TopDocuments { get; set; }
    }
}
