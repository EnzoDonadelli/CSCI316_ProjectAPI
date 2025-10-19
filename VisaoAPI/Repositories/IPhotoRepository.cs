using VisaoAPI.Models;

namespace VisaoAPI.Repositories
{
    public interface IPhotoRepository
    {
        Task<Photo?> GetByIdAsync(int id);
        Task<PhotoWithDetails?> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<PhotoWithDetails>> GetAllAsync();
        Task<IEnumerable<PhotoWithDetails>> GetByUserIdAsync(int userId);
        Task<IEnumerable<PhotoWithDetails>> GetByAlbumIdAsync(int albumId);
        Task<IEnumerable<PhotoWithDetails>> GetPhotosByTagAsync(string tagName, int limit = 25);
        Task<Photo> CreateAsync(Photo photo);
        Task<Photo> UpdateAsync(Photo photo);
        Task<bool> DeleteAsync(int id);
        Task<int> GetLikesCountAsync(int photoId);
        Task<int> GetCommentsCountAsync(int photoId);
        Task<IEnumerable<string>> GetPhotoTagsAsync(int photoId);
        Task<bool> ExistsAsync(int id);
    }
}