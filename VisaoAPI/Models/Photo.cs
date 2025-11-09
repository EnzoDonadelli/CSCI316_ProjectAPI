using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VisaoAPI.Models
{
    public class Photo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PhotoId { get; set; }

        [Required]
        public int UserId { get; set; }

        public int? AlbumId { get; set; }

        [StringLength(150)]
        public string? Title { get; set; }

        public string? Description { get; set; }

        [Required]
        [StringLength(255)]
        public string ImageUrl { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("AlbumId")]
        public virtual Album? Album { get; set; }

        public virtual ICollection<PhotoTag> PhotoTags { get; set; } = new List<PhotoTag>();
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}