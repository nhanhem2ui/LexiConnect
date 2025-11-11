using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects
{
    public class AIUsageLimit
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UsageLimitId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual Users User { get; set; }

        /// <summary>
        /// Total AI requests allowed per month (10 for free, NULL for unlimited premium)
        /// </summary>
        public int? MonthlyLimit { get; set; } = 10;

        /// <summary>
        /// Number of AI requests used this month
        /// </summary>
        public int UsedThisMonth { get; set; } = 0;

        /// <summary>
        /// Month and year this limit applies to (e.g., "2025-11")
        /// </summary>
        [Required]
        [MaxLength(7)]
        public string MonthYear { get; set; } = string.Empty;

        /// <summary>
        /// Last time the user made an AI request
        /// </summary>
        public DateTime? LastUsedAt { get; set; }

        /// <summary>
        /// Automatically resets on the first day of each month
        /// </summary>
        public DateTime? LastResetDate { get; set; }
    }
}
