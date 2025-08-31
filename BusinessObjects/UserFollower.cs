using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects
{
    public class UserFollower
    {
        public string FollowerId { get; set; }

        public string FollowingId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Users Follower { get; set; }
        public virtual Users Following { get; set; }
    }
}
