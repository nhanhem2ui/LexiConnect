using BusinessObjects;

namespace LexiConnect.Models.ViewModels
{
    public class AdminManagementViewModel
    {
        // Dashboard Statistics
        public int PendingDocuments { get; set; }
        public int TotalUsers { get; set; }
        public int TotalDocuments { get; set; }
        public int FlaggedDocuments { get; set; }
        public int ActiveSubscriptions { get; set; }
        public int TodaysUploads { get; set; }

        // Recent Data Collections
        public IEnumerable<Document>? RecentPendingDocuments { get; set; }
        public IEnumerable<Users>? RecentUsers { get; set; }
        public IEnumerable<Document>? FlaggedContent { get; set; }

        // Filter and Search Properties
        public string? SearchTerm { get; set; }
        public string? StatusFilter { get; set; }
        public string? UserTypeFilter { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
    }
}