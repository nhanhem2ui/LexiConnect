using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public class DocumentLike
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public string UserId { get; set; }
        public DateTime LikedAt { get; set; }

        // Navigation properties
        public virtual Document Document { get; set; }
        public virtual Users User { get; set; }
    }
}
