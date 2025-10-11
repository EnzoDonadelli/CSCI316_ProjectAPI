using VisaoAPI.Models;

namespace VisaoAPI.Repositories
{
    public interface IPhotoRepository
    {
        Task<Photo?> GetByIdAsync(int id);
        Task<IEnumerable<PhotoWithDetails>> GetAllAsync();
        Task<IEnumerable<PhotoWithDetails>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Photo>> GetByAlbumIdAsync(int albumId);
        Task<Photo> CreateAsync(Photo photo);
        Task<Photo> UpdateAsync(Photo photo);
        Task<bool> DeleteAsync(int id);
        Task<int> GetLikesCountAsync(int photoId);
        Task<int> GetCommentsCountAsync(int photoId);
        Task<IEnumerable<string>> GetPhotoTagsAsync(int photoId);
    }
}