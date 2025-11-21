using BusinessObjects;

namespace LexiConnect.Models.ViewModels
{
    public class AllRecentViewedsViewModel
    {
        public List<RecentViewed> RecentVieweds { get; set; } = new List<RecentViewed>();
        public Dictionary<int, bool> UserLikedDocuments { get; set; } = new Dictionary<int, bool>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string SearchTerm { get; set; } = "";
        public string SortBy { get; set; } = "recent";
        public int TotalRecentVieweds { get; set; }
    }
}

