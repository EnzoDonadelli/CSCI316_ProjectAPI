using VisaoAPI.Models;

namespace VisaoAPI.Repositories
{
    public interface IFollowerRepository
    {
        Task<Follower?> GetByIdAsync(int id);
        Task<Follower?> GetByFollowerAndFolloweeAsync(int followerId, int followeeId);
        Task<IEnumerable<Follower>> GetFollowersByUserIdAsync(int userId);
        Task<IEnumerable<Follower>> GetFollowingByUserIdAsync(int userId);
        Task<IEnumerable<User>> GetFollowersUsersAsync(int userId);
        Task<IEnumerable<User>> GetFollowingUsersAsync(int userId);
        Task<Follower> CreateAsync(Follower follower);
        Task<bool> DeleteAsync(int id);
        Task<bool> DeleteByFollowerAndFolloweeAsync(int followerId, int followeeId);
        Task<int> GetFollowersCountAsync(int userId);
        Task<int> GetFollowingCountAsync(int userId);
        Task<bool> ExistsAsync(int followerId, int followeeId);
    }
}