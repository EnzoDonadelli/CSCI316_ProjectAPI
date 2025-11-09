using VisaoAPI.Models;

namespace VisaoAPI.Repositories
{
    public interface ITagRepository
    {
        Task<Tag?> GetByIdAsync(int id);
        Task<Tag?> GetByNameAsync(string tagName);
        Task<IEnumerable<Tag>> GetAllAsync();
        Task<IEnumerable<Tag>> GetPopularTagsAsync(int count = 10);
        Task<IEnumerable<TagWithUsage>> GetPopularTagsWithUsageAsync(int count = 10);
        Task<Tag> CreateAsync(Tag tag);
        Task<Tag?> UpdateAsync(int id, Tag tag);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(string tagName);
        Task<int> GetUsageCountAsync(int tagId);
    }
}