using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects
{
    public class UserFollower
    {
        [Key, Column("follower_id", Order = 0)]
        public int FollowerId { get; set; }

        [Key, Column("following_id", Order = 1)]
        public int FollowingId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Users Follower { get; set; }
        public virtual Users Following { get; set; }
    }
}
