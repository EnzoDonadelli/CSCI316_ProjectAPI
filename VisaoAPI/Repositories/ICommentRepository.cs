using VisaoAPI.Models;

namespace VisaoAPI.Repositories
{
    public interface ICommentRepository
    {
        Task<Comment?> GetByIdAsync(int id);
        Task<CommentWithDetails?> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<Comment>> GetByPhotoIdAsync(int photoId);
        Task<IEnumerable<CommentWithDetails>> GetByPhotoIdWithDetailsAsync(int photoId);
        Task<IEnumerable<Comment>> GetByUserIdAsync(int userId);
        Task<Comment> CreateAsync(Comment comment);
        Task<Comment?> UpdateAsync(int id, Comment comment);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<int> GetCountByPhotoIdAsync(int photoId);
        Task<int> GetCountByUserIdAsync(int userId);
    }
}