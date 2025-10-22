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
    }
}