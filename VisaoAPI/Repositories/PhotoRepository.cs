using Microsoft.Data.SqlClient;
using Dapper;
using VisaoAPI.Models;
using System.Data;

namespace VisaoAPI.Repositories
{
    public class PhotoRepository : IPhotoRepository
    {
        private readonly string _connectionString;

        public PhotoRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string not found");
        }

        public async Task<Photo?> GetByIdAsync(int id)
        {
            const string sql = """
                SELECT PhotoId, UserId, AlbumId, Title, Description, ImageUrl, UploadedAt
                FROM Photos 
                WHERE PhotoId = @Id
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Photo>(sql, new { Id = id });
        }

        public async Task<PhotoWithDetails?> GetByIdWithDetailsAsync(int id)
        {
            const string sql = """
                SELECT p.PhotoId, p.UserId, p.AlbumId, p.Title, p.Description, p.ImageUrl, p.UploadedAt,
                       u.Username, a.Title as AlbumTitle
                FROM Photos p
                LEFT JOIN Users u ON p.UserId = u.UserId
                LEFT JOIN Albums a ON p.AlbumId = a.AlbumId
                WHERE p.PhotoId = @Id
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<PhotoWithDetails>(sql, new { Id = id });
        }

        public async Task<IEnumerable<PhotoWithDetails>> GetAllAsync()
        {
            const string sql = """
                SELECT p.PhotoId, p.UserId, p.AlbumId, p.Title, p.Description, p.ImageUrl, p.UploadedAt,
                       u.Username, a.Title as AlbumTitle
                FROM Photos p
                LEFT JOIN Users u ON p.UserId = u.UserId
                LEFT JOIN Albums a ON p.AlbumId = a.AlbumId
                ORDER BY p.UploadedAt DESC
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<PhotoWithDetails>(sql);
        }

        public async Task<IEnumerable<PhotoWithDetails>> GetByUserIdAsync(int userId)
        {
            const string sql = """
                SELECT p.PhotoId, p.UserId, p.AlbumId, p.Title, p.Description, p.ImageUrl, p.UploadedAt,
                       u.Username, a.Title as AlbumTitle
                FROM Photos p
                LEFT JOIN Users u ON p.UserId = u.UserId
                LEFT JOIN Albums a ON p.AlbumId = a.AlbumId
                WHERE p.UserId = @UserId
                ORDER BY p.UploadedAt DESC
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<PhotoWithDetails>(sql, new { UserId = userId });
        }

        public async Task<IEnumerable<PhotoWithDetails>> GetByAlbumIdAsync(int albumId)
        {
            const string sql = """
                SELECT p.PhotoId, p.UserId, p.AlbumId, p.Title, p.Description, p.ImageUrl, p.UploadedAt,
                       u.Username, a.Title as AlbumTitle
                FROM Photos p
                LEFT JOIN Users u ON p.UserId = u.UserId
                LEFT JOIN Albums a ON p.AlbumId = a.AlbumId
                WHERE p.AlbumId = @AlbumId
                ORDER BY p.UploadedAt DESC
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<PhotoWithDetails>(sql, new { AlbumId = albumId });
        }

        public async Task<IEnumerable<PhotoWithDetails>> GetPhotosByTagAsync(string tagName, int limit = 25)
        {
            const string sql = """
                SELECT TOP(@Limit) p.PhotoId, p.UserId, p.AlbumId, p.Title, p.Description, p.ImageUrl, p.UploadedAt,
                       u.Username, a.Title as AlbumTitle
                FROM Photos p
                INNER JOIN PhotoTags pt ON p.PhotoId = pt.PhotoId
                INNER JOIN Tags t ON pt.TagId = t.TagId
                LEFT JOIN Users u ON p.UserId = u.UserId
                LEFT JOIN Albums a ON p.AlbumId = a.AlbumId
                WHERE t.TagName = @TagName
                ORDER BY p.UploadedAt DESC
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<PhotoWithDetails>(sql, new { TagName = tagName, Limit = limit });
        }

        public async Task<Photo> CreateAsync(Photo photo)
        {
            const string sql = """
                INSERT INTO Photos (UserId, AlbumId, Title, Description, ImageUrl, UploadedAt)
                OUTPUT INSERTED.PhotoId, INSERTED.UserId, INSERTED.AlbumId, INSERTED.Title, 
                       INSERTED.Description, INSERTED.ImageUrl, INSERTED.UploadedAt
                VALUES (@UserId, @AlbumId, @Title, @Description, @ImageUrl, @UploadedAt)
                """;

            using var connection = new SqlConnection(_connectionString);
            var createdPhoto = await connection.QuerySingleAsync<Photo>(sql, photo);
            return createdPhoto;
        }

        public async Task<Photo> UpdateAsync(Photo photo)
        {
            const string sql = """
                UPDATE Photos 
                SET Title = @Title, Description = @Description, AlbumId = @AlbumId
                WHERE PhotoId = @PhotoId;
                
                SELECT p.PhotoId, p.UserId, p.AlbumId, p.Title, p.Description, p.ImageUrl, p.UploadedAt,
                       u.Username, a.Title as AlbumTitle
                FROM Photos p
                LEFT JOIN Users u ON p.UserId = u.UserId
                LEFT JOIN Albums a ON p.AlbumId = a.AlbumId
                WHERE p.PhotoId = @PhotoId
                """;

            using var connection = new SqlConnection(_connectionString);
            var updatedPhoto = await connection.QuerySingleAsync<Photo>(sql, photo);
            return updatedPhoto;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // Delete related records first (foreign key constraints)
                await connection.ExecuteAsync("DELETE FROM Comments WHERE PhotoId = @Id", new { Id = id }, transaction);
                await connection.ExecuteAsync("DELETE FROM Likes WHERE PhotoId = @Id", new { Id = id }, transaction);
                await connection.ExecuteAsync("DELETE FROM PhotoTags WHERE PhotoId = @Id", new { Id = id }, transaction);
                
                // Delete the photo
                var rowsAffected = await connection.ExecuteAsync("DELETE FROM Photos WHERE PhotoId = @Id", new { Id = id }, transaction);
                
                await transaction.CommitAsync();
                return rowsAffected > 0;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<int> GetLikesCountAsync(int photoId)
        {
            const string sql = "SELECT COUNT(*) FROM Likes WHERE PhotoId = @PhotoId";

            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleAsync<int>(sql, new { PhotoId = photoId });
        }

        public async Task<int> GetCommentsCountAsync(int photoId)
        {
            const string sql = "SELECT COUNT(*) FROM Comments WHERE PhotoId = @PhotoId";

            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleAsync<int>(sql, new { PhotoId = photoId });
        }

        public async Task<IEnumerable<string>> GetPhotoTagsAsync(int photoId)
        {
            const string sql = """
                SELECT t.TagName
                FROM PhotoTags pt
                INNER JOIN Tags t ON pt.TagId = t.TagId
                WHERE pt.PhotoId = @PhotoId
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<string>(sql, new { PhotoId = photoId });
        }

        public async Task<bool> ExistsAsync(int id)
        {
            const string sql = """
                SELECT COUNT(1) 
                FROM Photos 
                WHERE PhotoId = @Id
                """;

            using var connection = new SqlConnection(_connectionString);
            var count = await connection.QuerySingleAsync<int>(sql, new { Id = id });
            return count > 0;
        }
    }
}