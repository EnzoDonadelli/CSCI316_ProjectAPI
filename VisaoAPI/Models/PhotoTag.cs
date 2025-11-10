using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VisaoAPI.Models
{
    public class PhotoTag
    {
        [Required]
        public int PhotoId { get; set; }

        [Required]
        public int TagId { get; set; }

        // Navigation properties
        [ForeignKey("PhotoId")]
        public virtual Photo Photo { get; set; } = null!;

        [ForeignKey("TagId")]
        public virtual Tag Tag { get; set; } = null!;
    }
}