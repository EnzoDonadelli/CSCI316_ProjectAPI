using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisaoAPI.Data;
using VisaoAPI.Models;

namespace VisaoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FollowersController : ControllerBase
    {
        private readonly PhotoSharingDbContext _context;
        private readonly ILogger<FollowersController> _logger;

        public FollowersController(PhotoSharingDbContext context, ILogger<FollowersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Follow a user
        /// </summary>
        [HttpPost("{followerId}/follow/{followingId}")]
        public async Task<ActionResult> FollowUser(int followerId, int followingId)
        {
            if (followerId == followingId)
            {
                return BadRequest("Users cannot follow themselves");
            }

            var follower = await _context.Users.FindAsync(followerId);
            var following = await _context.Users.FindAsync(followingId);

            if (follower == null || following == null)
            {
                return BadRequest("One or both users not found");
            }

            var existingFollow = await _context.Followers
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);

            if (existingFollow != null)
            {
                return BadRequest("Already following this user");
            }

            var follow = new Follower
            {
                FollowerId = followerId,
                FollowingId = followingId,
                FollowedAt = DateTime.Now
            };

            _context.Followers.Add(follow);
            await _context.SaveChangesAsync();

            return Ok("Successfully followed user");
        }

        /// <summary>
        /// Unfollow a user
        /// </summary>
        [HttpDelete("{followerId}/unfollow/{followingId}")]
        public async Task<ActionResult> UnfollowUser(int followerId, int followingId)
        {
            var follow = await _context.Followers
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);

            if (follow == null)
            {
                return NotFound("Follow relationship not found");
            }

            _context.Followers.Remove(follow);
            await _context.SaveChangesAsync();

            return Ok("Successfully unfollowed user");
        }

        /// <summary>
        /// Get followers of a user
        /// </summary>
        [HttpGet("{userId}/followers")]
        public async Task<ActionResult<IEnumerable<object>>> GetFollowers(int userId)
        {
            var followers = await _context.Followers
                .Where(f => f.FollowingId == userId)
                .Include(f => f.FollowerUser)
                .Select(f => new
                {
                    UserId = f.FollowerUser.UserId,
                    Username = f.FollowerUser.Username,
                    FullName = f.FollowerUser.FullName,
                    ProfilePic = f.FollowerUser.ProfilePic,
                    FollowedAt = f.FollowedAt
                })
                .ToListAsync();

            return Ok(followers);
        }

        /// <summary>
        /// Get users that a user is following
        /// </summary>
        [HttpGet("{userId}/following")]
        public async Task<ActionResult<IEnumerable<object>>> GetFollowing(int userId)
        {
            var following = await _context.Followers
                .Where(f => f.FollowerId == userId)
                .Include(f => f.FollowingUser)
                .Select(f => new
                {
                    UserId = f.FollowingUser.UserId,
                    Username = f.FollowingUser.Username,
                    FullName = f.FollowingUser.FullName,
                    ProfilePic = f.FollowingUser.ProfilePic,
                    FollowedAt = f.FollowedAt
                })
                .ToListAsync();

            return Ok(following);
        }

        /// <summary>
        /// Check if user A follows user B
        /// </summary>
        [HttpGet("{followerId}/follows/{followingId}")]
        public async Task<ActionResult<bool>> CheckFollowStatus(int followerId, int followingId)
        {
            var follows = await _context.Followers
                .AnyAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);

            return Ok(follows);
        }

        /// <summary>
        /// Get follower statistics for a user
        /// </summary>
        [HttpGet("{userId}/stats")]
        public async Task<ActionResult<object>> GetFollowerStats(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var followersCount = await _context.Followers.CountAsync(f => f.FollowingId == userId);
            var followingCount = await _context.Followers.CountAsync(f => f.FollowerId == userId);

            var stats = new
            {
                UserId = userId,
                Username = user.Username,
                FollowersCount = followersCount,
                FollowingCount = followingCount
            };

            return Ok(stats);
        }
    }
}