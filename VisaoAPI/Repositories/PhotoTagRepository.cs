using Microsoft.Data.SqlClient;
using Dapper;
using VisaoAPI.Models;

namespace VisaoAPI.Repositories
{
    public class PhotoTagRepository : IPhotoTagRepository
    {
        private readonly string _connectionString;

        public PhotoTagRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string not found");
        }

        public async Task<PhotoTag?> GetByIdAsync(int id)
        {
            const string sql = """
                SELECT pt.PhotoId, pt.TagId,
                       p.Title as PhotoTitle, t.TagName
                FROM PhotoTags pt
                LEFT JOIN Photos p ON pt.PhotoId = p.PhotoId
                LEFT JOIN Tags t ON pt.TagId = t.TagId
                WHERE pt.PhotoId = @Id OR pt.TagId = @Id
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<PhotoTag>(sql, new { Id = id });
        }

        public async Task<PhotoTag?> GetByPhotoAndTagAsync(int photoId, int tagId)
        {
            const string sql = """
                SELECT PhotoId, TagId
                FROM PhotoTags 
                WHERE PhotoId = @PhotoId AND TagId = @TagId
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<PhotoTag>(sql, new { PhotoId = photoId, TagId = tagId });
        }

        public async Task<IEnumerable<PhotoTag>> GetByPhotoIdAsync(int photoId)
        {
            const string sql = """
                SELECT pt.PhotoId, pt.TagId,
                       t.TagName
                FROM PhotoTags pt
                LEFT JOIN Tags t ON pt.TagId = t.TagId
                WHERE pt.PhotoId = @PhotoId
                ORDER BY t.TagName
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<PhotoTag>(sql, new { PhotoId = photoId });
        }

        public async Task<IEnumerable<PhotoTag>> GetByTagIdAsync(int tagId)
        {
            const string sql = """
                SELECT pt.PhotoId, pt.TagId,
                       p.Title as PhotoTitle, p.ImageUrl
                FROM PhotoTags pt
                LEFT JOIN Photos p ON pt.PhotoId = p.PhotoId
                WHERE pt.TagId = @TagId
                ORDER BY p.UploadedAt DESC
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<PhotoTag>(sql, new { TagId = tagId });
        }

        public async Task<IEnumerable<PhotoWithDetails>> GetPhotosByTagNameAsync(string tagName, int limit = 25)
        {
            const string sql = """
                SELECT TOP(@Limit) p.PhotoId, p.UserId, p.AlbumId, p.Title, p.Description, p.ImageUrl, p.UploadedAt,
                       u.Username,
                       a.Title as AlbumTitle
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

        public async Task<IEnumerable<string>> GetTagNamesByPhotoIdAsync(int photoId)
        {
            const string sql = """
                SELECT t.TagName
                FROM PhotoTags pt
                INNER JOIN Tags t ON pt.TagId = t.TagId
                WHERE pt.PhotoId = @PhotoId
                ORDER BY t.TagName
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<string>(sql, new { PhotoId = photoId });
        }

        public async Task<PhotoTag> CreateAsync(PhotoTag photoTag)
        {
            const string sql = """
                INSERT INTO PhotoTags (PhotoId, TagId)
                OUTPUT INSERTED.PhotoId, INSERTED.TagId
                VALUES (@PhotoId, @TagId)
                """;

            using var connection = new SqlConnection(_connectionString);
            var createdPhotoTag = await connection.QuerySingleAsync<PhotoTag>(sql, photoTag);
            return createdPhotoTag;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = """
                DELETE FROM PhotoTags 
                WHERE PhotoId = @Id OR TagId = @Id
                """;

            using var connection = new SqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteByPhotoAndTagAsync(int photoId, int tagId)
        {
            const string sql = """
                DELETE FROM PhotoTags 
                WHERE PhotoId = @PhotoId AND TagId = @TagId
                """;

            using var connection = new SqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteAsync(sql, new { PhotoId = photoId, TagId = tagId });
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAllByPhotoIdAsync(int photoId)
        {
            const string sql = """
                DELETE FROM PhotoTags 
                WHERE PhotoId = @PhotoId
                """;

            using var connection = new SqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteAsync(sql, new { PhotoId = photoId });
            return rowsAffected > 0;
        }

        public async Task<bool> ExistsAsync(int photoId, int tagId)
        {
            const string sql = """
                SELECT COUNT(1) 
                FROM PhotoTags 
                WHERE PhotoId = @PhotoId AND TagId = @TagId
                """;

            using var connection = new SqlConnection(_connectionString);
            var count = await connection.QuerySingleAsync<int>(sql, new { PhotoId = photoId, TagId = tagId });
            return count > 0;
        }
    }
}