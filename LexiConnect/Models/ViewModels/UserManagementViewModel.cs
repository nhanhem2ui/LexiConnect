using BusinessObjects;

namespace LexiConnect.Models.ViewModels
{
    public class UserManagementViewModel
    {
        public IEnumerable<Users> Users { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }

        // Filter properties
        public string SearchTerm { get; set; }
        public string RoleFilter { get; set; }
        public string SubscriptionFilter { get; set; }
        public string UniversityFilter { get; set; }
        public string SortBy { get; set; }
        public string SortOrder { get; set; }

        // Statistics
        public int TotalUsers { get; set; }
        public int ActiveSubscriptions { get; set; }
        public int NewUsersThisMonth { get; set; }
    }
}