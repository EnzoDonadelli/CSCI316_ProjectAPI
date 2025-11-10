using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VisaoAPI.Models
{
    public class Like
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LikeId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int PhotoId { get; set; }

        public DateTime LikedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("PhotoId")]
        public virtual Photo Photo { get; set; } = null!;
    }
}