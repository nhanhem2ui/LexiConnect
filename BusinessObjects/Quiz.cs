using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects
{
    public class Quiz
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int QuizId { get; set; }

        [Required]
        public string CreatorId { get; set; } = string.Empty;

        [ForeignKey("CreatorId")]
        public virtual Users Creator { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Subject or topic of the quiz
        /// </summary>
        [MaxLength(100)]
        public string? Subject { get; set; }

        /// <summary>
        /// Difficulty level (Easy, Medium, Hard)
        /// </summary>
        [MaxLength(20)]
        public string Difficulty { get; set; } = "Medium";

        /// <summary>
        /// Optional course association
        /// </summary>
        public int? CourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course? Course { get; set; }

        /// <summary>
        /// Optional university association
        /// </summary>
        public int? UniversityId { get; set; }

        [ForeignKey("UniversityId")]
        public virtual University? University { get; set; }

        /// <summary>
        /// Whether the quiz is publicly visible
        /// </summary>
        public bool IsPublic { get; set; } = false;

        /// <summary>
        /// Whether the quiz is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Navigation to quiz questions
        /// </summary>
        public virtual ICollection<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();
    }
}
