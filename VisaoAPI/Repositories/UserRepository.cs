using Microsoft.Data.SqlClient;
using Dapper;
using VisaoAPI.Models;
using System.Data;

namespace VisaoAPI.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string not found");
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            const string sql = """
                SELECT UserId, Username, Email, PasswordHash, FullName, Bio, ProfilePic, CreatedAt 
                FROM Users 
                WHERE UserId = @Id
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            const string sql = """
                SELECT UserId, Username, Email, PasswordHash, FullName, Bio, ProfilePic, CreatedAt 
                FROM Users 
                WHERE Username = @Username
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Username = username });
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            const string sql = """
                SELECT UserId, Username, Email, PasswordHash, FullName, Bio, ProfilePic, CreatedAt 
                FROM Users 
                WHERE Email = @Email
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            const string sql = """
                SELECT UserId, Username, Email, PasswordHash, FullName, Bio, ProfilePic, CreatedAt 
                FROM Users 
                ORDER BY CreatedAt DESC
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<User>(sql);
        }

        public async Task<User> CreateAsync(User user)
        {
            const string sql = """
                INSERT INTO Users (Username, Email, PasswordHash, FullName, Bio, ProfilePic, CreatedAt)
                OUTPUT INSERTED.UserId, INSERTED.Username, INSERTED.Email, INSERTED.PasswordHash, 
                       INSERTED.FullName, INSERTED.Bio, INSERTED.ProfilePic, INSERTED.CreatedAt
                VALUES (@Username, @Email, @PasswordHash, @FullName, @Bio, @ProfilePic, @CreatedAt)
                """;

            using var connection = new SqlConnection(_connectionString);
            var createdUser = await connection.QuerySingleAsync<User>(sql, user);
            return createdUser;
        }

        public async Task<User> UpdateAsync(User user)
        {
            const string sql = """
                UPDATE Users 
                SET Username = @Username, Email = @Email, PasswordHash = @PasswordHash, 
                    FullName = @FullName, Bio = @Bio, ProfilePic = @ProfilePic
                WHERE UserId = @UserId;
                
                SELECT UserId, Username, Email, PasswordHash, FullName, Bio, ProfilePic, CreatedAt 
                FROM Users 
                WHERE UserId = @UserId
                """;

            using var connection = new SqlConnection(_connectionString);
            var updatedUser = await connection.QuerySingleAsync<User>(sql, user);
            return updatedUser;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = "DELETE FROM Users WHERE UserId = @Id";

            using var connection = new SqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            const string sql = "SELECT COUNT(1) FROM Users WHERE Username = @Username";

            using var connection = new SqlConnection(_connectionString);
            var count = await connection.QuerySingleAsync<int>(sql, new { Username = username });
            return count > 0;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            const string sql = "SELECT COUNT(1) FROM Users WHERE Email = @Email";

            using var connection = new SqlConnection(_connectionString);
            var count = await connection.QuerySingleAsync<int>(sql, new { Email = email });
            return count > 0;
        }

        public async Task<bool> UpdatePasswordAsync(int userId, string hashedPassword)
        {
            const string sql = "UPDATE Users SET PasswordHash = @HashedPassword WHERE UserId = @UserId";

            using var connection = new SqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteAsync(sql, new { UserId = userId, HashedPassword = hashedPassword });
            return rowsAffected > 0;
        }
    }
}