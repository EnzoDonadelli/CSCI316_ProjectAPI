using Microsoft.Data.SqlClient;
using Dapper;
using VisaoAPI.Models;

namespace VisaoAPI.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly string _connectionString;

        public CommentRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string not found");
        }

        public async Task<Comment?> GetByIdAsync(int id)
        {
            const string sql = """
                SELECT c.CommentId, c.PhotoId, c.UserId, c.CommentText, c.CommentedAt,
                       u.Username
                FROM Comments c
                LEFT JOIN Users u ON c.UserId = u.UserId
                WHERE c.CommentId = @Id
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Comment>(sql, new { Id = id });
        }

        public async Task<CommentWithDetails?> GetByIdWithDetailsAsync(int id)
        {
            const string sql = """
                SELECT c.CommentId, c.PhotoId, c.UserId, c.CommentText, c.CommentedAt,
                       u.Username
                FROM Comments c
                LEFT JOIN Users u ON c.UserId = u.UserId
                WHERE c.CommentId = @Id
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<CommentWithDetails>(sql, new { Id = id });
        }

        public async Task<IEnumerable<Comment>> GetByPhotoIdAsync(int photoId)
        {
            const string sql = """
                SELECT CommentId, PhotoId, UserId, CommentText, CommentedAt
                FROM Comments
                WHERE PhotoId = @PhotoId
                ORDER BY CommentedAt DESC
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Comment>(sql, new { PhotoId = photoId });
        }

        public async Task<IEnumerable<CommentWithDetails>> GetByPhotoIdWithDetailsAsync(int photoId)
        {
            const string sql = """
                SELECT c.CommentId, c.PhotoId, c.UserId, c.CommentText, c.CommentedAt,
                       u.Username
                FROM Comments c
                LEFT JOIN Users u ON c.UserId = u.UserId
                WHERE c.PhotoId = @PhotoId
                ORDER BY c.CommentedAt DESC
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<CommentWithDetails>(sql, new { PhotoId = photoId });
        }

        public async Task<IEnumerable<Comment>> GetByUserIdAsync(int userId)
        {
            const string sql = """
                SELECT c.CommentId, c.PhotoId, c.UserId, c.CommentText, c.CommentedAt,
                       u.Username
                FROM Comments c
                LEFT JOIN Users u ON c.UserId = u.UserId
                WHERE c.UserId = @UserId
                ORDER BY c.CommentedAt DESC
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Comment>(sql, new { UserId = userId });
        }

        public async Task<Comment> CreateAsync(Comment comment)
        {
            const string sql = """
                INSERT INTO Comments (PhotoId, UserId, CommentText, CommentedAt)
                OUTPUT INSERTED.CommentId, INSERTED.PhotoId, INSERTED.UserId, INSERTED.CommentText, INSERTED.CommentedAt
                VALUES (@PhotoId, @UserId, @CommentText, @CommentedAt)
                """;

            using var connection = new SqlConnection(_connectionString);
            var createdComment = await connection.QuerySingleAsync<Comment>(sql, comment);
            return createdComment;
        }

        public async Task<Comment?> UpdateAsync(int id, Comment comment)
        {
            const string sql = """
                UPDATE Comments 
                SET CommentText = @CommentText
                WHERE CommentId = @Id;
                
                SELECT c.CommentId, c.PhotoId, c.UserId, c.CommentText, c.CommentedAt,
                       u.Username
                FROM Comments c
                LEFT JOIN Users u ON c.UserId = u.UserId
                WHERE c.CommentId = @Id
                """;

            comment.CommentId = id;
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Comment>(sql, comment);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = """
                DELETE FROM Comments 
                WHERE CommentId = @Id
                """;

            using var connection = new SqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            const string sql = """
                SELECT COUNT(1) 
                FROM Comments 
                WHERE CommentId = @Id
                """;

            using var connection = new SqlConnection(_connectionString);
            var count = await connection.QuerySingleAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        public async Task<int> GetCountByPhotoIdAsync(int photoId)
        {
            const string sql = """
                SELECT COUNT(*) 
                FROM Comments 
                WHERE PhotoId = @PhotoId
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleAsync<int>(sql, new { PhotoId = photoId });
        }

        public async Task<int> GetCountByUserIdAsync(int userId)
        {
            const string sql = """
                SELECT COUNT(*) 
                FROM Comments 
                WHERE UserId = @UserId
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleAsync<int>(sql, new { UserId = userId });
        }
    }
}