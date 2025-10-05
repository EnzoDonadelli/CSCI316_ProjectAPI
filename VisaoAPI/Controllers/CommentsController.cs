using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisaoAPI.Data;
using VisaoAPI.DTOs;
using VisaoAPI.Models;

namespace VisaoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly PhotoSharingDbContext _context;
        private readonly ILogger<CommentsController> _logger;

        public CommentsController(PhotoSharingDbContext context, ILogger<CommentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get comments for a photo
        /// </summary>
        [HttpGet("photo/{photoId}")]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetPhotoComments(int photoId)
        {
            var photo = await _context.Photos.FindAsync(photoId);
            if (photo == null)
            {
                return NotFound("Photo not found");
            }

            var comments = await _context.Comments
                .Where(c => c.PhotoId == photoId)
                .Include(c => c.User)
                .Select(c => new CommentDto
                {
                    CommentId = c.CommentId,
                    PhotoId = c.PhotoId,
                    UserId = c.UserId,
                    Username = c.User.Username,
                    CommentText = c.CommentText,
                    CommentedAt = c.CommentedAt
                })
                .OrderByDescending(c => c.CommentedAt)
                .ToListAsync();

            return Ok(comments);
        }

        /// <summary>
        /// Add a comment to a photo
        /// </summary>
        [HttpPost("photo/{photoId}/user/{userId}")]
        public async Task<ActionResult<CommentDto>> AddComment(int photoId, int userId, CreateCommentDto createCommentDto)
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

            var comment = new Comment
            {
                PhotoId = photoId,
                UserId = userId,
                CommentText = createCommentDto.CommentText,
                CommentedAt = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var commentDto = new CommentDto
            {
                CommentId = comment.CommentId,
                PhotoId = comment.PhotoId,
                UserId = comment.UserId,
                Username = user.Username,
                CommentText = comment.CommentText,
                CommentedAt = comment.CommentedAt
            };

            return CreatedAtAction(nameof(GetComment), new { id = comment.CommentId }, commentDto);
        }

        /// <summary>
        /// Get a comment by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<CommentDto>> GetComment(int id)
        {
            var comment = await _context.Comments
                .Include(c => c.User)
                .Where(c => c.CommentId == id)
                .Select(c => new CommentDto
                {
                    CommentId = c.CommentId,
                    PhotoId = c.PhotoId,
                    UserId = c.UserId,
                    Username = c.User.Username,
                    CommentText = c.CommentText,
                    CommentedAt = c.CommentedAt
                })
                .FirstOrDefaultAsync();

            if (comment == null)
            {
                return NotFound();
            }

            return Ok(comment);
        }

        /// <summary>
        /// Delete a comment
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}