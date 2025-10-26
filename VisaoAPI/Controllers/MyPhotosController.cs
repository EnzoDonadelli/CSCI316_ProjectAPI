using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VisaoAPI.DTOs;
using VisaoAPI.Repositories;
using System.Security.Claims;

namespace VisaoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MyPhotosController : ControllerBase
    {
        private readonly IPhotoRepository _photoRepository;
        private readonly ILogger<MyPhotosController> _logger;

        public MyPhotosController(IPhotoRepository photoRepository, ILogger<MyPhotosController> logger)
        {
            _photoRepository = photoRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get current user's photos only (requires authentication)
        /// </summary>
        [HttpGet]
        [Authorize] // This ensures only authenticated users can access this endpoint
        public async Task<ActionResult<IEnumerable<PhotoDto>>> GetMyPhotos()
        {
            try
            {
                // Extract the user ID from the JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized("Invalid user token");
                }

                // Only get photos belonging to the authenticated user using repository
                var photos = await _photoRepository.GetByUserIdAsync(userId);

                var photoDtos = photos.Select(p => new PhotoDto
                {
                    PhotoId = p.PhotoId,
                    UserId = p.UserId,
                    Username = p.Username, // Repository already includes username
                    AlbumId = p.AlbumId,
                    AlbumTitle = p.AlbumTitle, // Repository already includes album title
                    Title = p.Title,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    UploadedAt = p.UploadedAt
                }).ToList();

                return Ok(photoDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user's photos");
                return StatusCode(500, "An error occurred while retrieving photos");
            }
        }

        /// <summary>
        /// Update a photo (only if it belongs to the authenticated user)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult> UpdateMyPhoto(int id, [FromBody] UpdatePhotoDto updatePhotoDto)
        {
            try
            {
                // Extract user ID from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized("Invalid user token");
                }

                // Find the photo using repository
                var photo = await _photoRepository.GetByIdAsync(id);
                if (photo == null)
                {
                    return NotFound("Photo not found");
                }

                // Check if the photo belongs to the authenticated user
                if (photo.UserId != userId)
                {
                    return Forbid("You can only modify your own photos"); // 403 Forbidden
                }

                // Update the photo using repository
                photo.Title = updatePhotoDto.Title ?? photo.Title;
                photo.Description = updatePhotoDto.Description ?? photo.Description;

                await _photoRepository.UpdateAsync(photo);

                return Ok(new { message = "Photo updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating photo {PhotoId}", id);
                return StatusCode(500, "An error occurred while updating the photo");
            }
        }

        /// <summary>
        /// Delete a photo (only if it belongs to the authenticated user)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteMyPhoto(int id)
        {
            try
            {
                // Extract user ID from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized("Invalid user token");
                }

                // Find the photo using repository
                var photo = await _photoRepository.GetByIdAsync(id);
                if (photo == null)
                {
                    return NotFound("Photo not found");
                }

                // Check if the photo belongs to the authenticated user
                if (photo.UserId != userId)
                {
                    return Forbid("You can only delete your own photos"); // 403 Forbidden
                }

                // Delete the photo using repository
                await _photoRepository.DeleteAsync(id);

                return Ok(new { message = "Photo deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting photo {PhotoId}", id);
                return StatusCode(500, "An error occurred while deleting the photo");
            }
        }
    }
}