using Microsoft.AspNetCore.Mvc;
using VisaoAPI.Models;
using VisaoAPI.Repositories;

namespace VisaoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FollowersController : ControllerBase
    {
        private readonly IFollowerRepository _followerRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<FollowersController> _logger;

        public FollowersController(
            IFollowerRepository followerRepository,
            IUserRepository userRepository,
            ILogger<FollowersController> logger)
        {
            _followerRepository = followerRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        /// <summary>
        /// Follow a user
        /// </summary>
        [HttpPost("{followerId}/follow/{followingId}")]
        public async Task<ActionResult> FollowUser(int followerId, int followingId)
        {
            try
            {
                if (followerId == followingId)
                {
                    return BadRequest("Users cannot follow themselves");
                }

                var followerExists = await _userRepository.ExistsAsync(followerId);
                var followingExists = await _userRepository.ExistsAsync(followingId);

                if (!followerExists || !followingExists)
                {
                    return BadRequest("One or both users not found");
                }

                var existingFollow = await _followerRepository.GetByFollowerAndFolloweeAsync(followerId, followingId);
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

                await _followerRepository.CreateAsync(follow);

                return Ok("Successfully followed user");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error following user: {FollowerId} -> {FollowingId}", followerId, followingId);
                return StatusCode(500, "An error occurred while following the user");
            }
        }

        /// <summary>
        /// Unfollow a user
        /// </summary>
        [HttpDelete("{followerId}/unfollow/{followingId}")]
        public async Task<ActionResult> UnfollowUser(int followerId, int followingId)
        {
            try
            {
                var deleted = await _followerRepository.DeleteByFollowerAndFolloweeAsync(followerId, followingId);
                if (!deleted)
                {
                    return NotFound("Follow relationship not found");
                }

                return Ok("Successfully unfollowed user");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unfollowing user: {FollowerId} -> {FollowingId}", followerId, followingId);
                return StatusCode(500, "An error occurred while unfollowing the user");
            }
        }

        /// <summary>
        /// Get followers of a user
        /// </summary>
        [HttpGet("{userId}/followers")]
        public async Task<ActionResult<IEnumerable<object>>> GetFollowers(int userId)
        {
            try
            {
                var userExists = await _userRepository.ExistsAsync(userId);
                if (!userExists)
                {
                    return NotFound("User not found");
                }

                var followers = await _followerRepository.GetFollowersUsersAsync(userId);
                
                var followerDtos = followers.Select(f => new
                {
                    UserId = f.UserId,
                    Username = f.Username,
                    FullName = f.FullName,
                    ProfilePic = f.ProfilePic
                }).ToList();

                return Ok(followerDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting followers for user: {UserId}", userId);
                return StatusCode(500, "An error occurred while retrieving followers");
            }
        }

        /// <summary>
        /// Get users that a user is following
        /// </summary>
        [HttpGet("{userId}/following")]
        public async Task<ActionResult<IEnumerable<object>>> GetFollowing(int userId)
        {
            try
            {
                var userExists = await _userRepository.ExistsAsync(userId);
                if (!userExists)
                {
                    return NotFound("User not found");
                }

                var following = await _followerRepository.GetFollowingUsersAsync(userId);
                
                var followingDtos = following.Select(f => new
                {
                    UserId = f.UserId,
                    Username = f.Username,
                    FullName = f.FullName,
                    ProfilePic = f.ProfilePic
                }).ToList();

                return Ok(followingDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting following for user: {UserId}", userId);
                return StatusCode(500, "An error occurred while retrieving following list");
            }
        }

        /// <summary>
        /// Check if user A follows user B
        /// </summary>
        [HttpGet("{followerId}/follows/{followingId}")]
        public async Task<ActionResult<bool>> CheckFollowStatus(int followerId, int followingId)
        {
            try
            {
                var follows = await _followerRepository.ExistsAsync(followerId, followingId);
                return Ok(follows);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking follow status: {FollowerId} -> {FollowingId}", followerId, followingId);
                return StatusCode(500, "An error occurred while checking follow status");
            }
        }

        /// <summary>
        /// Get follower statistics for a user
        /// </summary>
        [HttpGet("{userId}/stats")]
        public async Task<ActionResult<object>> GetFollowerStats(int userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                var followersCount = await _followerRepository.GetFollowersCountAsync(userId);
                var followingCount = await _followerRepository.GetFollowingCountAsync(userId);

                var stats = new
                {
                    UserId = userId,
                    Username = user.Username,
                    FollowersCount = followersCount,
                    FollowingCount = followingCount
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting follower stats for user: {UserId}", userId);
                return StatusCode(500, "An error occurred while retrieving follower statistics");
            }
        }
    }
}