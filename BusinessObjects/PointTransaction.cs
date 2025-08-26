using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace BusinessObjects
{
    public class PointTransaction
    {
        [Key]
        [Column("transaction_id")]
        public int TransactionId { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("points_change")]
        // -- positive for earned, negative for spent
        public int PointsChange { get; set; }

        [Required]
        [MaxLength(30)]
        [Column("transaction_type")]
        public string TransactionType { get; set; }

        [Column("reference_id")]
        //-- document_id, subscription_id, etc.
        public int? ReferenceId { get; set; }

        [MaxLength(20)]
        [Column("reference_type")]
        //-- 'document', 'subscription', 'bonus', 'penalty'
        public string ReferenceType { get; set; }

        [MaxLength(200)]
        [Column("description")]
        public string Description { get; set; }

        [Required]
        [Column("balance_after")]
        public int BalanceAfter { get; set; }

        [Column("expires_at")]
        public DateTime? ExpiresAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property to User
        public virtual Users User { get; set; }
    }
}