using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects
{
    public class RecentViewed
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(University))]
        public int DocumentId { get; set; } = -100;

        public virtual Document? Document { get; set; }

        [ForeignKey(nameof(Course))]
        public int CourseId { get; set; } = -100;
        public virtual Course? Course { get; set; }
    }
}
