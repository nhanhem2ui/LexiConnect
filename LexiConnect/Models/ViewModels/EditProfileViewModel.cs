using BusinessObjects;
using System.ComponentModel.DataAnnotations;

namespace LexiConnect.Models.ViewModels
{
    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "Full name is required")]
        [Display(Name = "Full Name")]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "University")]
        public int? UniversityId { get; set; }

        [Display(Name = "Major")]
        public int? MajorId { get; set; }

        // Current avatar URL (for display purposes)
        public string AvatarUrl { get; set; } = "~/image/default-avatar.png";

        // File upload for new avatar
        [Display(Name = "Profile Picture")]
        public IFormFile? AvatarFile { get; set; }

        // Read-only properties for display
        public int PointsBalance { get; set; }
        public int TotalPointsEarned { get; set; }
        public int DocumentCount { get; set; }

        // Subscription information (read-only)
        public SubscriptionPlan? SubscriptionPlan { get; set; }
        public DateTime? SubscriptionStartDate { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }

        public EditProfileViewModel(Users user, int documentCount = 0)
        {
            FullName = user.FullName ?? string.Empty;
            Email = user.Email ?? string.Empty;
            PhoneNumber = user.PhoneNumber;
            UniversityId = user.UniversityId;
            MajorId = user.MajorId;
            AvatarUrl = user.AvatarUrl;
            PointsBalance = user.PointsBalance;
            TotalPointsEarned = user.TotalPointsEarned;
            DocumentCount = documentCount;
            SubscriptionPlan = user.SubscriptionPlan;
            SubscriptionStartDate = user.SubscriptionStartDate;
            SubscriptionEndDate = user.SubscriptionEndDate;
        }
    }
}