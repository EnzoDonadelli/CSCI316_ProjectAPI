using Microsoft.Data.SqlClient;
using Dapper;
using VisaoAPI.Models;

namespace VisaoAPI.Repositories
{
    public class LikeRepository : ILikeRepository
    {
        private readonly string _connectionString;

        public LikeRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string not found");
        }

        public async Task<Like?> GetByIdAsync(int id)
        {
            const string sql = """
                SELECT LikeId, PhotoId, UserId, LikedAt
                FROM Likes 
                WHERE LikeId = @Id
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Like>(sql, new { Id = id });
        }

        public async Task<LikeWithDetails?> GetByIdWithDetailsAsync(int id)
        {
            const string sql = """
                SELECT l.LikeId, l.PhotoId, l.UserId, l.LikedAt,
                       u.Username
                FROM Likes l
                LEFT JOIN Users u ON l.UserId = u.UserId
                WHERE l.LikeId = @Id
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<LikeWithDetails>(sql, new { Id = id });
        }

        public async Task<Like?> GetByUserAndPhotoAsync(int userId, int photoId)
        {
            const string sql = """
                SELECT LikeId, PhotoId, UserId, LikedAt
                FROM Likes 
                WHERE UserId = @UserId AND PhotoId = @PhotoId
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Like>(sql, new { UserId = userId, PhotoId = photoId });
        }

        public async Task<IEnumerable<Like>> GetByPhotoIdAsync(int photoId)
        {
            const string sql = """
                SELECT LikeId, PhotoId, UserId, LikedAt
                FROM Likes
                WHERE PhotoId = @PhotoId
                ORDER BY LikedAt DESC
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Like>(sql, new { PhotoId = photoId });
        }

        public async Task<IEnumerable<LikeWithDetails>> GetByPhotoIdWithDetailsAsync(int photoId)
        {
            const string sql = """
                SELECT l.LikeId, l.PhotoId, l.UserId, l.LikedAt,
                       u.Username
                FROM Likes l
                LEFT JOIN Users u ON l.UserId = u.UserId
                WHERE l.PhotoId = @PhotoId
                ORDER BY l.LikedAt DESC
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<LikeWithDetails>(sql, new { PhotoId = photoId });
        }

        public async Task<IEnumerable<Like>> GetByUserIdAsync(int userId)
        {
            const string sql = """
                SELECT l.LikeId, l.PhotoId, l.UserId, l.LikedAt,
                       p.Title as PhotoTitle, p.ImageUrl
                FROM Likes l
                LEFT JOIN Photos p ON l.PhotoId = p.PhotoId
                WHERE l.UserId = @UserId
                ORDER BY l.LikedAt DESC
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Like>(sql, new { UserId = userId });
        }

        public async Task<IEnumerable<User>> GetUsersWhoLikedPhotoAsync(int photoId)
        {
            const string sql = """
                SELECT u.UserId, u.Username, u.Email, u.FullName, u.Bio, u.ProfilePic, u.CreatedAt
                FROM Users u
                INNER JOIN Likes l ON u.UserId = l.UserId
                WHERE l.PhotoId = @PhotoId
                ORDER BY l.LikedAt DESC
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<User>(sql, new { PhotoId = photoId });
        }

        public async Task<Like> CreateAsync(Like like)
        {
            const string sql = """
                INSERT INTO Likes (PhotoId, UserId, LikedAt)
                OUTPUT INSERTED.LikeId, INSERTED.PhotoId, INSERTED.UserId, INSERTED.LikedAt
                VALUES (@PhotoId, @UserId, @LikedAt)
                """;

            using var connection = new SqlConnection(_connectionString);
            var createdLike = await connection.QuerySingleAsync<Like>(sql, like);
            return createdLike;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = """
                DELETE FROM Likes 
                WHERE LikeId = @Id
                """;

            using var connection = new SqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteByUserAndPhotoAsync(int userId, int photoId)
        {
            const string sql = """
                DELETE FROM Likes 
                WHERE UserId = @UserId AND PhotoId = @PhotoId
                """;

            using var connection = new SqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteAsync(sql, new { UserId = userId, PhotoId = photoId });
            return rowsAffected > 0;
        }

        public async Task<int> GetCountByPhotoIdAsync(int photoId)
        {
            const string sql = """
                SELECT COUNT(*) 
                FROM Likes 
                WHERE PhotoId = @PhotoId
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleAsync<int>(sql, new { PhotoId = photoId });
        }

        public async Task<int> GetCountByUserIdAsync(int userId)
        {
            const string sql = """
                SELECT COUNT(*) 
                FROM Likes 
                WHERE UserId = @UserId
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleAsync<int>(sql, new { UserId = userId });
        }

        public async Task<bool> ExistsAsync(int userId, int photoId)
        {
            const string sql = """
                SELECT COUNT(1) 
                FROM Likes 
                WHERE UserId = @UserId AND PhotoId = @PhotoId
                """;

            using var connection = new SqlConnection(_connectionString);
            var count = await connection.QuerySingleAsync<int>(sql, new { UserId = userId, PhotoId = photoId });
            return count > 0;
        }
    }
}