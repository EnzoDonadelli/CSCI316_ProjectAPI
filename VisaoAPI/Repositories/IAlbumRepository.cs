using VisaoAPI.Models;

namespace VisaoAPI.Repositories
{
    public interface IAlbumRepository
    {
        Task<Album?> GetByIdAsync(int id);
        Task<AlbumWithDetails?> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<AlbumWithDetails>> GetAllAsync();
        Task<IEnumerable<AlbumWithDetails>> GetByUserIdAsync(int userId);
        Task<Album> CreateAsync(Album album);
        Task<Album> UpdateAsync(Album album);
        Task<bool> DeleteAsync(int id);
        Task<int> GetPhotosCountAsync(int albumId);
    }
}