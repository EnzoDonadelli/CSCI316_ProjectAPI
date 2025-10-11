using Microsoft.Data.SqlClient;
using Dapper;
using VisaoAPI.Models;
using System.Data;

namespace VisaoAPI.Repositories
{
    public class AlbumRepository : IAlbumRepository
    {
        private readonly string _connectionString;

        public AlbumRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string not found");
        }

        public async Task<Album?> GetByIdAsync(int id)
        {
            const string sql = """
                SELECT AlbumId, UserId, Title, Description, CreatedAt
                FROM Albums 
                WHERE AlbumId = @Id
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Album>(sql, new { Id = id });
        }

        public async Task<AlbumWithDetails?> GetByIdWithDetailsAsync(int id)
        {
            const string sql = """
                SELECT a.AlbumId, a.UserId, a.Title, a.Description, a.CreatedAt,
                       u.Username,
                       (SELECT COUNT(*) FROM Photos p WHERE p.AlbumId = a.AlbumId) as PhotosCount
                FROM Albums a
                LEFT JOIN Users u ON a.UserId = u.UserId
                WHERE a.AlbumId = @Id
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<AlbumWithDetails>(sql, new { Id = id });
        }

        public async Task<IEnumerable<AlbumWithDetails>> GetAllAsync()
        {
            const string sql = """
                SELECT a.AlbumId, a.UserId, a.Title, a.Description, a.CreatedAt,
                       u.Username,
                       (SELECT COUNT(*) FROM Photos p WHERE p.AlbumId = a.AlbumId) as PhotosCount
                FROM Albums a
                LEFT JOIN Users u ON a.UserId = u.UserId
                ORDER BY a.CreatedAt DESC
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<AlbumWithDetails>(sql);
        }

        public async Task<IEnumerable<AlbumWithDetails>> GetByUserIdAsync(int userId)
        {
            const string sql = """
                SELECT a.AlbumId, a.UserId, a.Title, a.Description, a.CreatedAt,
                       u.Username,
                       (SELECT COUNT(*) FROM Photos p WHERE p.AlbumId = a.AlbumId) as PhotosCount
                FROM Albums a
                LEFT JOIN Users u ON a.UserId = u.UserId
                WHERE a.UserId = @UserId
                ORDER BY a.CreatedAt DESC
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<AlbumWithDetails>(sql, new { UserId = userId });
        }

        public async Task<Album> CreateAsync(Album album)
        {
            const string sql = """
                INSERT INTO Albums (UserId, Title, Description, CreatedAt)
                OUTPUT INSERTED.AlbumId, INSERTED.UserId, INSERTED.Title, 
                       INSERTED.Description, INSERTED.CreatedAt
                VALUES (@UserId, @Title, @Description, @CreatedAt)
                """;

            using var connection = new SqlConnection(_connectionString);
            var createdAlbum = await connection.QuerySingleAsync<Album>(sql, album);
            return createdAlbum;
        }

        public async Task<Album> UpdateAsync(Album album)
        {
            const string sql = """
                UPDATE Albums 
                SET Title = @Title, Description = @Description
                WHERE AlbumId = @AlbumId;
                
                SELECT a.AlbumId, a.UserId, a.Title, a.Description, a.CreatedAt,
                       u.Username
                FROM Albums a
                LEFT JOIN Users u ON a.UserId = u.UserId
                WHERE a.AlbumId = @AlbumId
                """;

            using var connection = new SqlConnection(_connectionString);
            var updatedAlbum = await connection.QuerySingleAsync<Album>(sql, album);
            return updatedAlbum;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // Update photos to remove album association (set AlbumId to NULL)
                await connection.ExecuteAsync("UPDATE Photos SET AlbumId = NULL WHERE AlbumId = @Id", new { Id = id }, transaction);
                
                // Delete the album
                var rowsAffected = await connection.ExecuteAsync("DELETE FROM Albums WHERE AlbumId = @Id", new { Id = id }, transaction);
                
                await transaction.CommitAsync();
                return rowsAffected > 0;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<int> GetPhotosCountAsync(int albumId)
        {
            const string sql = "SELECT COUNT(*) FROM Photos WHERE AlbumId = @AlbumId";

            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleAsync<int>(sql, new { AlbumId = albumId });
        }
    }
}