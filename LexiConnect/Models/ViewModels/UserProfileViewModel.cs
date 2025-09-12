using BusinessObjects;

namespace LexiConnect.Models.ViewModels
{
    public class UserProfileViewModel
    {
        public required Users User { get; set; }
        public IQueryable<Document>? Documents { get; set; }
        public IQueryable<RecentViewed>? RecentActivities { get; set; }
        public int Upvotes {  get; set; }
        public int FollowerNum { get; set; }
    }
}
