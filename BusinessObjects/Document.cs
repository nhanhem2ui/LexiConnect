using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects
{
    public class Document
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DocumentId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(20)]
        // Values: notes, quiz, assignment, exam, study_guide, flashcards
        public string DocumentType { get; set; } = "notes";

        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        public long FileSize { get; set; } // in bytes

        [Required]
        [MaxLength(10)]
        public string FileType { get; set; } = string.Empty; // pdf, doc, etc.

        [MaxLength(500)]
        public string? ThumbnailUrl { get; set; }

        // Relationships
        [ForeignKey(nameof(Uploader))]
        public string UploaderId { get; set; } = string.Empty;  // FK -> IdentityUser (string key)
        public virtual Users Uploader { get; set; }

        [ForeignKey(nameof(Course))]
        public int CourseId { get; set; }
        public virtual Course Course { get; set; }

        [ForeignKey(nameof(ApprovedByUser))]
        public string? ApprovedBy { get; set; } // FK -> IdentityUser
        public virtual Users? ApprovedByUser { get; set; }

        // Stats
        public int PointsAwarded { get; set; } = 0;
        public int PointsToDownload { get; set; } = 0;
        public int DownloadCount { get; set; } = 0;
        public int ViewCount { get; set; } = 0;
        public int LikeCount { get; set; } = 0;

        public string? FilePDFpath { get; set; }
        // Flags
        public bool IsPremiumOnly { get; set; } = false;
        public bool IsAiGenerated { get; set; } = false;

        [Column(TypeName = "decimal(3,2)")]
        public decimal? AiConfidenceScore { get; set; } // 0.00 - 1.00

        [MaxLength(20)]
        public string Status { get; set; } = "pending";
        // pending, approved, rejected, flagged, processing

        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        public DateTime? ApprovedAt { get; set; }

        [MaxLength(5)]
        public string LanguageCode { get; set; } = "en";

        public int? PageCount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Many-to-many: Tags
        public virtual ICollection<DocumentTag> Tags { get; set; } = new List<DocumentTag>();
    }
}
