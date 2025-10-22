using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects
{
    public class PaymentRecord
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        public string UserId { get; set; } // Foreign key to Identity user

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string Currency { get; set; } = "VND";

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = "VnPay";

        [Required]
        public string Status { get; set; } = "Pending"; //  Pending, Processing,Completed, Failed, Cancelled, Refunded

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ProcessedAt { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        // Navigation property to Identity user
        [ForeignKey(nameof(UserId))]
        public virtual Users User { get; set; }
    }
}
