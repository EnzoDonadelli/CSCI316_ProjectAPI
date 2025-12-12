namespace VisaoAPI.DTOs
{
    public class AlbumDto
    {
        public int AlbumId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public int PhotosCount { get; set; }
    }

    public class CreateAlbumDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class UpdateAlbumDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
    }
}