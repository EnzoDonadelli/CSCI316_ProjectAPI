using VisaoAPI.Models;

namespace VisaoAPI.Repositories
{
    public interface ILikeRepository
    {
        Task<Like?> GetByIdAsync(int id);
        Task<LikeWithDetails?> GetByIdWithDetailsAsync(int id);
        Task<Like?> GetByUserAndPhotoAsync(int userId, int photoId);
        Task<IEnumerable<Like>> GetByPhotoIdAsync(int photoId);
        Task<IEnumerable<LikeWithDetails>> GetByPhotoIdWithDetailsAsync(int photoId);
        Task<IEnumerable<Like>> GetByUserIdAsync(int userId);
        Task<IEnumerable<User>> GetUsersWhoLikedPhotoAsync(int photoId);
        Task<Like> CreateAsync(Like like);
        Task<bool> DeleteAsync(int id);
        Task<bool> DeleteByUserAndPhotoAsync(int userId, int photoId);
        Task<int> GetCountByPhotoIdAsync(int photoId);
        Task<int> GetCountByUserIdAsync(int userId);
        Task<bool> ExistsAsync(int userId, int photoId);
    }
}