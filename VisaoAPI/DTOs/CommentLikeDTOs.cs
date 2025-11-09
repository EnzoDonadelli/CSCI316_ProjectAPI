namespace VisaoAPI.DTOs
{
    public class CommentDto
    {
        public int CommentId { get; set; }
        public int PhotoId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string CommentText { get; set; } = string.Empty;
        public DateTime CommentedAt { get; set; }
    }

    public class CreateCommentDto
    {
        public string CommentText { get; set; } = string.Empty;
    }

    public class LikeDto
    {
        public int LikeId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public int PhotoId { get; set; }
        public DateTime LikedAt { get; set; }
    }
}