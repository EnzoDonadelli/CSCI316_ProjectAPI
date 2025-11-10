using Microsoft.Data.SqlClient;
using Dapper;
using VisaoAPI.Models;

namespace VisaoAPI.Repositories
{
    public class FollowerRepository : IFollowerRepository
    {
        private readonly string _connectionString;

        public FollowerRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string not found");
        }

    public async Task<Follower?> GetByIdAsync(int id)
    {
        const string sql = """
        SELECT FollowerId, FollowingId, FollowedAt
        FROM Followers 
        WHERE FollowerId = @Id OR FollowingId = @Id
        """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Follower>(sql, new { Id = id });
        }

    public async Task<Follower?> GetByFollowerAndFolloweeAsync(int followerId, int followeeId)
    {
        const string sql = """
        SELECT FollowerId, FollowingId, FollowedAt
        FROM Followers 
        WHERE FollowerId = @FollowerId AND FollowingId = @FolloweeId
        """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Follower>(sql, new { FollowerId = followerId, FolloweeId = followeeId });
        }

    public async Task<IEnumerable<Follower>> GetFollowersByUserIdAsync(int userId)
    {
        const string sql = """
        SELECT FollowerId, FollowingId, FollowedAt
        FROM Followers 
        WHERE FollowingId = @UserId
        ORDER BY FollowedAt DESC
        """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Follower>(sql, new { UserId = userId });
        }

    public async Task<IEnumerable<Follower>> GetFollowingByUserIdAsync(int userId)
        {
            const string sql = """
                SELECT FollowerId, FollowingId, FollowedAt
                FROM Followers 
                WHERE FollowerId = @UserId
                ORDER BY FollowedAt DESC
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Follower>(sql, new { UserId = userId });
        }

    public async Task<IEnumerable<User>> GetFollowersUsersAsync(int userId)
        {
            const string sql = """
                SELECT u.UserId, u.Username, u.Email, u.FullName, u.Bio, u.ProfilePic, u.CreatedAt
                FROM Users u
                INNER JOIN Followers f ON u.UserId = f.FollowerId
                WHERE f.FollowingId = @UserId
                ORDER BY f.FollowedAt DESC
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<User>(sql, new { UserId = userId });
        }

    public async Task<IEnumerable<User>> GetFollowingUsersAsync(int userId)
        {
            const string sql = """
                SELECT u.UserId, u.Username, u.Email, u.FullName, u.Bio, u.ProfilePic, u.CreatedAt
                FROM Users u
                INNER JOIN Followers f ON u.UserId = f.FollowingId
                WHERE f.FollowerId = @UserId
                ORDER BY f.FollowedAt DESC
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<User>(sql, new { UserId = userId });
        }

    public async Task<Follower> CreateAsync(Follower follower)
        {
            const string sql = """
                INSERT INTO Followers (FollowerId, FollowingId, FollowedAt)
                OUTPUT INSERTED.FollowerId, INSERTED.FollowingId, INSERTED.FollowedAt
                VALUES (@FollowerId, @FollowingId, @FollowedAt)
                """;

            using var connection = new SqlConnection(_connectionString);
            var createdFollower = await connection.QuerySingleAsync<Follower>(sql, follower);
            return createdFollower;
        }

    public async Task<bool> DeleteAsync(int id)
        {
            const string sql = """
                DELETE FROM Followers 
                WHERE FollowerId = @Id OR FollowingId = @Id
                """;

            using var connection = new SqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
            return rowsAffected > 0;
        }

    public async Task<bool> DeleteByFollowerAndFolloweeAsync(int followerId, int followeeId)
        {
            const string sql = """
                DELETE FROM Followers 
                WHERE FollowerId = @FollowerId AND FollowingId = @FolloweeId
                """;

            using var connection = new SqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteAsync(sql, new { FollowerId = followerId, FolloweeId = followeeId });
            return rowsAffected > 0;
        }

    public async Task<int> GetFollowersCountAsync(int userId)
        {
            const string sql = """
                SELECT COUNT(*) 
                FROM Followers 
                WHERE FollowingId = @UserId
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleAsync<int>(sql, new { UserId = userId });
        }

        public async Task<int> GetFollowingCountAsync(int userId)
        {
            const string sql = """
                SELECT COUNT(*) 
                FROM Followers 
                WHERE FollowerId = @UserId
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleAsync<int>(sql, new { UserId = userId });
        }

    public async Task<bool> ExistsAsync(int followerId, int followeeId)
        {
            const string sql = """
                SELECT COUNT(1) 
                FROM Followers 
                WHERE FollowerId = @FollowerId AND FollowingId = @FolloweeId
                """;

            using var connection = new SqlConnection(_connectionString);
            var count = await connection.QuerySingleAsync<int>(sql, new { FollowerId = followerId, FolloweeId = followeeId });
            return count > 0;
        }
    }
}