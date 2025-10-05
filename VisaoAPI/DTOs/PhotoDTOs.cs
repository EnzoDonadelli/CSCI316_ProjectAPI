namespace VisaoAPI.DTOs
{
    public class PhotoDto
    {
        public int PhotoId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public int? AlbumId { get; set; }
        public string? AlbumTitle { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public List<string> Tags { get; set; } = new();
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
    }

    public class CreatePhotoDto
    {
        public int? AlbumId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
    }

    public class UpdatePhotoDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int? AlbumId { get; set; }
        public List<string> Tags { get; set; } = new();
    }
}