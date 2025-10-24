using BusinessObjects;

namespace LexiConnect.Models.ViewModels
{
    public class PublicUserProfileViewModel
    {
        // User Information
        public Users User { get; set; }

        // Statistics
        public int TotalUploads { get; set; }
        public int TotalUpvotes { get; set; }
        public int TotalComments { get; set; }
        public int StudentsHelped { get; set; }
        public int FollowerCount { get; set; }
        public int FollowingCount { get; set; }

        // Follow Status (for current logged-in user)
        public bool IsFollowing { get; set; }
        public bool IsOwnProfile { get; set; }

        // Popular Uploads
        public List<DocumentWithStats> PopularDocuments { get; set; } = new List<DocumentWithStats>();

        // Recent Activity
        public List<RecentActivity> RecentActivities { get; set; } = new List<RecentActivity>();

        // Nested class for document with additional stats
        public class DocumentWithStats
        {
            public Document Document { get; set; }
            public int Rating { get; set; } // Percentage (0-100)
            public int TotalRatings { get; set; }
        }

        // Nested class for recent activities
        public class RecentActivity
        {
            public string ActivityType { get; set; } // "upload", "comment", "like"
            public string Title { get; set; }
            public string Description { get; set; }
            public DateTime CreatedAt { get; set; }
            public string RelativeTime { get; set; }
        }
    }
}