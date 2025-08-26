using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects
{
    public class UserFavorite
    {
        [Key, Column("user_id", Order = 0)]
        public int UserId { get; set; }

        [Key, Column("document_id", Order = 1)]
        public int DocumentId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Users User { get; set; }
        public virtual Document Document { get; set; }
    }
}
