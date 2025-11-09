namespace VisaoAPI.Models
{
    /// <summary>
    /// Album model with additional details from joined tables (User, Photos count)
    /// Used by repositories when fetching albums with related data
    /// </summary>
    public class AlbumWithDetails
    {
        public int AlbumId { get; set; }
        public int UserId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Additional properties from joined tables
        public string Username { get; set; } = string.Empty;
        public int PhotosCount { get; set; }
    }
}