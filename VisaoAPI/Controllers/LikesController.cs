using Microsoft.AspNetCore.Mvc;
using VisaoAPI.DTOs;
using VisaoAPI.Models;
using VisaoAPI.Repositories;

namespace VisaoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LikesController : ControllerBase
    {
        private readonly ILikeRepository _likeRepository;
        private readonly IPhotoRepository _photoRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<LikesController> _logger;

        public LikesController(
            ILikeRepository likeRepository,
            IPhotoRepository photoRepository,
            IUserRepository userRepository,
            ILogger<LikesController> logger)
        {
            _likeRepository = likeRepository;
            _photoRepository = photoRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get likes for a photo
        /// </summary>
        [HttpGet("photo/{photoId}")]
        public async Task<ActionResult<IEnumerable<LikeDto>>> GetPhotoLikes(int photoId)
        {
            try
            {
                var photoExists = await _photoRepository.ExistsAsync(photoId);
                if (!photoExists)
                {
                    return NotFound("Photo not found");
                }

                var likes = await _likeRepository.GetByPhotoIdWithDetailsAsync(photoId);

                var likeDtos = likes.Select(l => new LikeDto
                {
                    LikeId = l.LikeId,
                    UserId = l.UserId,
                    Username = l.Username,
                    PhotoId = l.PhotoId,
                    LikedAt = l.LikedAt
                }).ToList();

                return Ok(likeDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting likes for photo: {PhotoId}", photoId);
                return StatusCode(500, "An error occurred while retrieving likes");
            }
        }

        /// <summary>
        /// Like a photo
        /// </summary>
        [HttpPost("photo/{photoId}/user/{userId}")]
        public async Task<ActionResult<LikeDto>> LikePhoto(int photoId, int userId)
        {
            try
            {
                var photoExists = await _photoRepository.ExistsAsync(photoId);
                if (!photoExists)
                {
                    return NotFound("Photo not found");
                }

                var userExists = await _userRepository.ExistsAsync(userId);
                if (!userExists)
                {
                    return BadRequest("User not found");
                }

                // Check if user already liked this photo
                var existingLike = await _likeRepository.GetByUserAndPhotoAsync(userId, photoId);
                if (existingLike != null)
                {
                    return BadRequest("User has already liked this photo");
                }

                var like = new Like
                {
                    UserId = userId,
                    PhotoId = photoId,
                    LikedAt = DateTime.Now
                };

                var createdLike = await _likeRepository.CreateAsync(like);

                // Get user details for response
                var user = await _userRepository.GetByIdAsync(userId);

                var likeDto = new LikeDto
                {
                    LikeId = createdLike.LikeId,
                    UserId = createdLike.UserId,
                    Username = user?.Username ?? "",
                    PhotoId = createdLike.PhotoId,
                    LikedAt = createdLike.LikedAt
                };

                return CreatedAtAction(nameof(GetLike), new { id = createdLike.LikeId }, likeDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error liking photo: {PhotoId} by user: {UserId}", photoId, userId);
                return StatusCode(500, "An error occurred while liking the photo");
            }
        }

        /// <summary>
        /// Unlike a photo
        /// </summary>
        [HttpDelete("photo/{photoId}/user/{userId}")]
        public async Task<IActionResult> UnlikePhoto(int photoId, int userId)
        {
            try
            {
                var deleted = await _likeRepository.DeleteByUserAndPhotoAsync(userId, photoId);
                if (!deleted)
                {
                    return NotFound("Like not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unliking photo: {PhotoId} by user: {UserId}", photoId, userId);
                return StatusCode(500, "An error occurred while unliking the photo");
            }
        }

        /// <summary>
        /// Get a like by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<LikeDto>> GetLike(int id)
        {
            try
            {
                var like = await _likeRepository.GetByIdWithDetailsAsync(id);

                if (like == null)
                {
                    return NotFound();
                }

                var likeDto = new LikeDto
                {
                    LikeId = like.LikeId,
                    UserId = like.UserId,
                    Username = like.Username,
                    PhotoId = like.PhotoId,
                    LikedAt = like.LikedAt
                };

                return Ok(likeDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting like: {LikeId}", id);
                return StatusCode(500, "An error occurred while retrieving the like");
            }
        }

        /// <summary>
        /// Check if user liked a photo
        /// </summary>
        [HttpGet("photo/{photoId}/user/{userId}/check")]
        public async Task<ActionResult<bool>> CheckUserLikedPhoto(int photoId, int userId)
        {
            try
            {
                var like = await _likeRepository.GetByUserAndPhotoAsync(userId, photoId);
                return Ok(like != null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user liked photo: {PhotoId}, user: {UserId}", photoId, userId);
                return StatusCode(500, "An error occurred while checking like status");
            }
        }

        /// <summary>
        /// Get likes count for a photo
        /// </summary>
        [HttpGet("photo/{photoId}/count")]
        public async Task<ActionResult<int>> GetPhotoLikesCount(int photoId)
        {
            try
            {
                var photoExists = await _photoRepository.ExistsAsync(photoId);
                if (!photoExists)
                {
                    return NotFound("Photo not found");
                }

                var count = await _likeRepository.GetCountByPhotoIdAsync(photoId);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting likes count for photo: {PhotoId}", photoId);
                return StatusCode(500, "An error occurred while retrieving likes count");
            }
        }
    }
}