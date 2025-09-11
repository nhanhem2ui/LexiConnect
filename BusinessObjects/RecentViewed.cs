using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects
{
    public class RecentViewed
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(Document))]
        public int DocumentId { get; set; } = -100;
        public virtual Document? Document { get; set; }

        [ForeignKey(nameof(Course))]
        public int CourseId { get; set; } = -100;
        public virtual Course? Course { get; set; }

        [ForeignKey(nameof(User))]
        public string UserId { get; set; } = string.Empty;
        public virtual Users User { get; set; }

        public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
    }
}
