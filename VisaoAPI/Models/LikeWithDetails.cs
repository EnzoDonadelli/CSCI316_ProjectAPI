namespace VisaoAPI.Models
{
    public class LikeWithDetails
    {
        public int LikeId { get; set; }
        public int PhotoId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public DateTime LikedAt { get; set; }
    }
}