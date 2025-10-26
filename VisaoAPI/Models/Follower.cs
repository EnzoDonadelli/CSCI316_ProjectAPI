using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VisaoAPI.Models
{
    public class Follower
    {
        [Required]
        public int FollowerId { get; set; }

        [Required]
        public int FollowingId { get; set; }

        public DateTime FollowedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("FollowerId")]
        public virtual User FollowerUser { get; set; } = null!;

        [ForeignKey("FollowingId")]
        public virtual User FollowingUser { get; set; } = null!;
    }
}