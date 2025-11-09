using Microsoft.Data.SqlClient;
using Dapper;
using VisaoAPI.Models;

namespace VisaoAPI.Repositories
{
    public class TagRepository : ITagRepository
    {
        private readonly string _connectionString;

        public TagRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string not found");
        }

        public async Task<Tag?> GetByIdAsync(int id)
        {
            const string sql = """
                SELECT TagId, TagName
                FROM Tags 
                WHERE TagId = @Id
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Tag>(sql, new { Id = id });
        }

        public async Task<Tag?> GetByNameAsync(string tagName)
        {
            const string sql = """
                SELECT TagId, TagName
                FROM Tags 
                WHERE TagName = @TagName
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Tag>(sql, new { TagName = tagName });
        }

        public async Task<IEnumerable<Tag>> GetAllAsync()
        {
            const string sql = """
                SELECT TagId, TagName
                FROM Tags 
                ORDER BY TagName
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Tag>(sql);
        }

        public async Task<IEnumerable<TagWithUsage>> GetPopularTagsWithUsageAsync(int count = 10)
        {
            const string sql = """
                SELECT TOP(@Count) t.TagId, t.TagName, COUNT(pt.PhotoId) as UsageCount
                FROM Tags t
                LEFT JOIN PhotoTags pt ON t.TagId = pt.TagId
                GROUP BY t.TagId, t.TagName
                ORDER BY COUNT(pt.PhotoId) DESC, t.TagName
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<TagWithUsage>(sql, new { Count = count });
        }

        public async Task<IEnumerable<Tag>> GetPopularTagsAsync(int count = 10)
        {
            const string sql = """
                SELECT TOP(@Count) t.TagId, t.TagName, COUNT(pt.PhotoId) as UsageCount
                FROM Tags t
                LEFT JOIN PhotoTags pt ON t.TagId = pt.TagId
                GROUP BY t.TagId, t.TagName
                ORDER BY COUNT(pt.PhotoId) DESC, t.TagName
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Tag>(sql, new { Count = count });
        }

        public async Task<Tag> CreateAsync(Tag tag)
        {
            const string sql = """
                INSERT INTO Tags (TagName)
                OUTPUT INSERTED.TagId, INSERTED.TagName
                VALUES (@TagName)
                """;

            using var connection = new SqlConnection(_connectionString);
            var createdTag = await connection.QuerySingleAsync<Tag>(sql, tag);
            return createdTag;
        }

        public async Task<Tag?> UpdateAsync(int id, Tag tag)
        {
            const string sql = """
                UPDATE Tags 
                SET TagName = @TagName
                WHERE TagId = @Id;
                
                SELECT TagId, TagName
                FROM Tags 
                WHERE TagId = @Id
                """;

            tag.TagId = id;
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Tag>(sql, tag);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = """
                DELETE FROM Tags 
                WHERE TagId = @Id
                """;

            using var connection = new SqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<bool> ExistsAsync(string tagName)
        {
            const string sql = """
                SELECT COUNT(1) 
                FROM Tags 
                WHERE TagName = @TagName
                """;

            using var connection = new SqlConnection(_connectionString);
            var count = await connection.QuerySingleAsync<int>(sql, new { TagName = tagName });
            return count > 0;
        }

        public async Task<int> GetUsageCountAsync(int tagId)
        {
            const string sql = """
                SELECT COUNT(*) 
                FROM PhotoTags 
                WHERE TagId = @TagId
                """;

            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleAsync<int>(sql, new { TagId = tagId });
        }
    }
}