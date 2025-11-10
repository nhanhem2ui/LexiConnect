using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public class QuizQuestion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int QuestionId { get; set; }

        [Required]
        public int QuizId { get; set; }

        [ForeignKey("QuizId")]
        public virtual Quiz Quiz { get; set; }

        /// <summary>
        /// Order of question in the quiz (1, 2, 3, etc.)
        /// </summary>
        public int QuestionOrder { get; set; }

        /// <summary>
        /// The actual question text
        /// </summary>
        [Required]
        public string QuestionText { get; set; } = string.Empty;

        /// <summary>
        /// Question type (MultipleChoice, TrueFalse)
        /// </summary>
        [MaxLength(20)]
        public string QuestionType { get; set; } = "MultipleChoice";

        /// <summary>
        /// Points awarded for correct answer
        /// </summary>
        public int Points { get; set; } = 1;

        /// <summary>
        /// Option A text
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string OptionA { get; set; } = string.Empty;

        /// <summary>
        /// Option B text
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string OptionB { get; set; } = string.Empty;

        /// <summary>
        /// Option C text (optional)
        /// </summary>
        [MaxLength(500)]
        public string? OptionC { get; set; }

        /// <summary>
        /// Option D text (optional)
        /// </summary>
        [MaxLength(500)]
        public string? OptionD { get; set; }

        /// <summary>
        /// Correct answer (A, B, C, or D)
        /// </summary>
        [Required]
        [MaxLength(1)]
        public string CorrectAnswer { get; set; } = string.Empty;

        /// <summary>
        /// Optional explanation for the correct answer
        /// </summary>
        [MaxLength(1000)]
        public string? Explanation { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
