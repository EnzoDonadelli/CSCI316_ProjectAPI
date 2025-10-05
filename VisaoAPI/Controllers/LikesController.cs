using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisaoAPI.Data;
using VisaoAPI.DTOs;
using VisaoAPI.Models;

namespace VisaoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LikesController : ControllerBase
    {
        private readonly PhotoSharingDbContext _context;
        private readonly ILogger<LikesController> _logger;

        public LikesController(PhotoSharingDbContext context, ILogger<LikesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get likes for a photo
        /// </summary>
        [HttpGet("photo/{photoId}")]
        public async Task<ActionResult<IEnumerable<LikeDto>>> GetPhotoLikes(int photoId)
        {
            var photo = await _context.Photos.FindAsync(photoId);
            if (photo == null)
            {
                return NotFound("Photo not found");
            }

            var likes = await _context.Likes
                .Where(l => l.PhotoId == photoId)
                .Include(l => l.User)
                .Select(l => new LikeDto
                {
                    LikeId = l.LikeId,
                    UserId = l.UserId,
                    Username = l.User.Username,
                    PhotoId = l.PhotoId,
                    LikedAt = l.LikedAt
                })
                .OrderByDescending(l => l.LikedAt)
                .ToListAsync();

            return Ok(likes);
        }

        /// <summary>
        /// Like a photo
        /// </summary>
        [HttpPost("photo/{photoId}/user/{userId}")]
        public async Task<ActionResult<LikeDto>> LikePhoto(int photoId, int userId)
        {
            var photo = await _context.Photos.FindAsync(photoId);
            if (photo == null)
            {
                return NotFound("Photo not found");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            // Check if user already liked this photo
            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.UserId == userId && l.PhotoId == photoId);

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

            _context.Likes.Add(like);
            await _context.SaveChangesAsync();

            var likeDto = new LikeDto
            {
                LikeId = like.LikeId,
                UserId = like.UserId,
                Username = user.Username,
                PhotoId = like.PhotoId,
                LikedAt = like.LikedAt
            };

            return CreatedAtAction(nameof(GetLike), new { id = like.LikeId }, likeDto);
        }

        /// <summary>
        /// Unlike a photo
        /// </summary>
        [HttpDelete("photo/{photoId}/user/{userId}")]
        public async Task<IActionResult> UnlikePhoto(int photoId, int userId)
        {
            var like = await _context.Likes
                .FirstOrDefaultAsync(l => l.UserId == userId && l.PhotoId == photoId);

            if (like == null)
            {
                return NotFound("Like not found");
            }

            _context.Likes.Remove(like);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Get a like by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<LikeDto>> GetLike(int id)
        {
            var like = await _context.Likes
                .Include(l => l.User)
                .Where(l => l.LikeId == id)
                .Select(l => new LikeDto
                {
                    LikeId = l.LikeId,
                    UserId = l.UserId,
                    Username = l.User.Username,
                    PhotoId = l.PhotoId,
                    LikedAt = l.LikedAt
                })
                .FirstOrDefaultAsync();

            if (like == null)
            {
                return NotFound();
            }

            return Ok(like);
        }

        /// <summary>
        /// Check if user liked a photo
        /// </summary>
        [HttpGet("photo/{photoId}/user/{userId}/check")]
        public async Task<ActionResult<bool>> CheckUserLikedPhoto(int photoId, int userId)
        {
            var liked = await _context.Likes
                .AnyAsync(l => l.UserId == userId && l.PhotoId == photoId);

            return Ok(liked);
        }

        /// <summary>
        /// Get likes count for a photo
        /// </summary>
        [HttpGet("photo/{photoId}/count")]
        public async Task<ActionResult<int>> GetPhotoLikesCount(int photoId)
        {
            var photo = await _context.Photos.FindAsync(photoId);
            if (photo == null)
            {
                return NotFound("Photo not found");
            }

            var count = await _context.Likes.CountAsync(l => l.PhotoId == photoId);
            return Ok(count);
        }
    }
}