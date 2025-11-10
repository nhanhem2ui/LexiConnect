using BusinessObjects;

namespace LexiConnect.Models.ViewModels
{
    public class AllDocumentsViewModel
    {
        public List<Document> Documents { get; set; } = new List<Document>();
        public Dictionary<int, bool> UserLikedDocuments { get; set; } = new Dictionary<int, bool>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string SearchTerm { get; set; } = "";
        public string SortBy { get; set; } = "recent";
        public int TotalDocuments { get; set; }
    }
}
