namespace VisaoAPI.Models
{
    public class CommentWithDetails
    {
        public int CommentId { get; set; }
        public int PhotoId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string CommentText { get; set; } = string.Empty;
        public DateTime CommentedAt { get; set; }
    }
}