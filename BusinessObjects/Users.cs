using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects
{
    public class Users : IdentityUser
    {
        [Required]
        public string FullName { get; set; }
        public string AvatarUrl { get; set; } = "default-avatar.png";

        public int UniversityId { get; set; }
        public virtual University University { get; set; }

        public int MajorId { get; set; }
        public virtual Major Major { get; set; }

        public int PointsBalance { get; set; }
        public int TotalPointsEarned { get; set; }

        public int SubscriptionPlanId { get; set; }
        public virtual SubscriptionPlan SubscriptionPlan { get; set; }

        public DateTime? SubscriptionStartDate { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }
    }
}
