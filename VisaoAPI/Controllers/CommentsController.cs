using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using VisaoAPI.DTOs;
using VisaoAPI.Models;
using VisaoAPI.Repositories;
using System.Security.Claims;

namespace VisaoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IPhotoRepository _photoRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<CommentsController> _logger;

        public CommentsController(
            ICommentRepository commentRepository,
            IPhotoRepository photoRepository,
            IUserRepository userRepository,
            ILogger<CommentsController> logger)
        {
            _commentRepository = commentRepository;
            _photoRepository = photoRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get comments for a photo
        /// </summary>
        [HttpGet("photo/{photoId}")]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetPhotoComments(int photoId)
        {
            try
            {
                var photoExists = await _photoRepository.ExistsAsync(photoId);
                if (!photoExists)
                {
                    return NotFound("Photo not found");
                }

                var comments = await _commentRepository.GetByPhotoIdWithDetailsAsync(photoId);

                var commentDtos = comments.Select(c => new CommentDto
                {
                    CommentId = c.CommentId,
                    PhotoId = c.PhotoId,
                    UserId = c.UserId,
                    Username = c.Username,
                    CommentText = c.CommentText,
                    CommentedAt = c.CommentedAt
                }).ToList();

                return Ok(commentDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comments for photo: {PhotoId}", photoId);
                return StatusCode(500, "An error occurred while retrieving comments");
            }
        }

        /// <summary>
        /// Add a comment to a photo
        /// </summary>
        [HttpPost("photo/{photoId}/user/{userId}")]
        public async Task<ActionResult<CommentDto>> AddComment(int photoId, int userId, CreateCommentDto createCommentDto)
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

                var comment = new Comment
                {
                    PhotoId = photoId,
                    UserId = userId,
                    CommentText = createCommentDto.CommentText,
                    CommentedAt = DateTime.Now
                };

                var createdComment = await _commentRepository.CreateAsync(comment);

                // Get user details for response
                var user = await _userRepository.GetByIdAsync(userId);

                var commentDto = new CommentDto
                {
                    CommentId = createdComment.CommentId,
                    PhotoId = createdComment.PhotoId,
                    UserId = createdComment.UserId,
                    Username = user?.Username ?? "",
                    CommentText = createdComment.CommentText,
                    CommentedAt = createdComment.CommentedAt
                };

                return CreatedAtAction(nameof(GetComment), new { id = createdComment.CommentId }, commentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment to photo: {PhotoId}", photoId);
                return StatusCode(500, "An error occurred while adding the comment");
            }
        }

        /// <summary>
        /// Get a comment by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<CommentDto>> GetComment(int id)
        {
            try
            {
                var comment = await _commentRepository.GetByIdWithDetailsAsync(id);

                if (comment == null)
                {
                    return NotFound();
                }

                var commentDto = new CommentDto
                {
                    CommentId = comment.CommentId,
                    PhotoId = comment.PhotoId,
                    UserId = comment.UserId,
                    Username = comment.Username,
                    CommentText = comment.CommentText,
                    CommentedAt = comment.CommentedAt
                };

                return Ok(commentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comment: {CommentId}", id);
                return StatusCode(500, "An error occurred while retrieving the comment");
            }
        }

        /// <summary>
        /// Delete a comment
        /// </summary>
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            try
            {
                var comment = await _commentRepository.GetByIdAsync(id);
                if (comment == null)
                {
                    return NotFound();
                }

                // Ensure the authenticated user is the commenter or is admin
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var authUserId))
                {
                    return Unauthorized("Invalid user token");
                }
                var isAdmin = User.IsInRole("admin") || User.Claims.Any(c => c.Type == "role" && c.Value == "admin");
                if (comment.UserId != authUserId && !isAdmin)
                {
                    return Forbid();
                }

                var deleted = await _commentRepository.DeleteAsync(id);
                if (!deleted)
                {
                    return StatusCode(500, "Failed to delete comment");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment: {CommentId}", id);
                return StatusCode(500, "An error occurred while deleting the comment");
            }
        }
    }
}