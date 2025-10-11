using System.ComponentModel.DataAnnotations;

namespace VisaoAPI.Models
{
    /// <summary>
    /// Photo model with additional details from joined tables (User, Album)
    /// Used by repositories when fetching photos with related data
    /// </summary>
    public class PhotoWithDetails
    {
        public int PhotoId { get; set; }
        public int UserId { get; set; }
        public int? AlbumId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        
        // Additional properties from joined tables
        public string Username { get; set; } = string.Empty;
        public string? AlbumTitle { get; set; }
    }
}