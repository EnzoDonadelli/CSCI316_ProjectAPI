using VisaoAPI.Models;

namespace VisaoAPI.Repositories
{
    public interface IPhotoTagRepository
    {
        Task<PhotoTag?> GetByIdAsync(int id);
        Task<PhotoTag?> GetByPhotoAndTagAsync(int photoId, int tagId);
        Task<IEnumerable<PhotoTag>> GetByPhotoIdAsync(int photoId);
        Task<IEnumerable<PhotoTag>> GetByTagIdAsync(int tagId);
        Task<IEnumerable<PhotoWithDetails>> GetPhotosByTagNameAsync(string tagName, int limit = 25);
        Task<IEnumerable<string>> GetTagNamesByPhotoIdAsync(int photoId);
        Task<PhotoTag> CreateAsync(PhotoTag photoTag);
        Task<bool> DeleteAsync(int id);
        Task<bool> DeleteByPhotoAndTagAsync(int photoId, int tagId);
        Task<bool> DeleteAllByPhotoIdAsync(int photoId);
        Task<bool> ExistsAsync(int photoId, int tagId);
    }
}