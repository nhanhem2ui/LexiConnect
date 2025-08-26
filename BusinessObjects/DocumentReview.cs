using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects
{
    public class DocumentReview
    {
        [Key]
        [Column("review_id")]
        public int ReviewId { get; set; }

        [Required]
        [Column("document_id")]
        public int DocumentId { get; set; }

        [Required]
        [Column("reviewer_id")]
        public int ReviewerId { get; set; }

        [Required]
        [Column("rating")]
        public byte Rating { get; set; } // 1-5 stars

        [MaxLength(1000)]
        [Column("comment")]
        public string? Comment { get; set; }

        [Column("helpful_count")]
        public int HelpfulCount { get; set; } = 0;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Document Document { get; set; }
        public virtual Users Reviewer { get; set; }
    }
}
