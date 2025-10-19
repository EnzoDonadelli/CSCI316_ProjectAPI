using Microsoft.AspNetCore.Mvc;
using VisaoAPI.DTOs;
using VisaoAPI.Models;
using VisaoAPI.Repositories;
using System.Security.Claims;

namespace VisaoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PhotosController : ControllerBase
    {
        private readonly IPhotoRepository _photoRepository;
        private readonly ILikeRepository _likeRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IPhotoTagRepository _photoTagRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAlbumRepository _albumRepository;
        private readonly ITagRepository _tagRepository;
        private readonly ILogger<PhotosController> _logger;

        public PhotosController(
            IPhotoRepository photoRepository, 
            ILikeRepository likeRepository,
            ICommentRepository commentRepository,
            IPhotoTagRepository photoTagRepository,
            IUserRepository userRepository,
            IAlbumRepository albumRepository,
            ITagRepository tagRepository,
            ILogger<PhotosController> logger)
        {
            _photoRepository = photoRepository;
            _likeRepository = likeRepository;
            _commentRepository = commentRepository;
            _photoTagRepository = photoTagRepository;
            _userRepository = userRepository;
            _albumRepository = albumRepository;
            _tagRepository = tagRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get all photos
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PhotoDto>>> GetPhotos()
        {
            try
            {
                var photos = await _photoRepository.GetAllAsync();
                
                var photoDtos = new List<PhotoDto>();
                foreach (var photo in photos)
                {
                    var tags = await _photoTagRepository.GetTagNamesByPhotoIdAsync(photo.PhotoId);
                    var likesCount = await _likeRepository.GetCountByPhotoIdAsync(photo.PhotoId);
                    var commentsCount = await _commentRepository.GetCountByPhotoIdAsync(photo.PhotoId);
                    
                    photoDtos.Add(new PhotoDto
                    {
                        PhotoId = photo.PhotoId,
                        UserId = photo.UserId,
                        Username = photo.Username,
                        AlbumId = photo.AlbumId,
                        AlbumTitle = photo.AlbumTitle,
                        Title = photo.Title,
                        Description = photo.Description,
                        ImageUrl = photo.ImageUrl,
                        UploadedAt = photo.UploadedAt,
                        Tags = tags.ToList(),
                        LikesCount = likesCount,
                        CommentsCount = commentsCount
                    });
                }

                return Ok(photoDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all photos");
                return StatusCode(500, "An error occurred while retrieving photos");
            }
        }

        /// <summary>
        /// Get first 25 photos by tag name
        /// </summary>
        [HttpGet("tag/{tagName}")]
        public async Task<ActionResult<IEnumerable<PhotoDto>>> GetPhotosByTag(string tagName, [FromQuery] int limit = 25)
        {
            try
            {
                var photos = await _photoRepository.GetPhotosByTagAsync(tagName, limit);
                
                var photoDtos = new List<PhotoDto>();
                foreach (var photo in photos)
                {
                    var tags = await _photoTagRepository.GetTagNamesByPhotoIdAsync(photo.PhotoId);
                    var likesCount = await _likeRepository.GetCountByPhotoIdAsync(photo.PhotoId);
                    var commentsCount = await _commentRepository.GetCountByPhotoIdAsync(photo.PhotoId);
                    
                    photoDtos.Add(new PhotoDto
                    {
                        PhotoId = photo.PhotoId,
                        UserId = photo.UserId,
                        Username = photo.Username,
                        AlbumId = photo.AlbumId,
                        AlbumTitle = photo.AlbumTitle,
                        Title = photo.Title,
                        Description = photo.Description,
                        ImageUrl = photo.ImageUrl,
                        UploadedAt = photo.UploadedAt,
                        Tags = tags.ToList(),
                        LikesCount = likesCount,
                        CommentsCount = commentsCount
                    });
                }

                return Ok(photoDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting photos by tag: {TagName}", tagName);
                return StatusCode(500, "An error occurred while retrieving photos by tag");
            }
        }

        /// <summary>
        /// Get photos by album ID
        /// </summary>
        [HttpGet("album/{albumId}")]
        public async Task<ActionResult<IEnumerable<PhotoDto>>> GetPhotosByAlbum(int albumId)
        {
            try
            {
                var photos = await _photoRepository.GetByAlbumIdAsync(albumId);
                
                var photoDtos = new List<PhotoDto>();
                foreach (var photo in photos)
                {
                    var tags = await _photoTagRepository.GetTagNamesByPhotoIdAsync(photo.PhotoId);
                    var likesCount = await _likeRepository.GetCountByPhotoIdAsync(photo.PhotoId);
                    var commentsCount = await _commentRepository.GetCountByPhotoIdAsync(photo.PhotoId);
                    
                    photoDtos.Add(new PhotoDto
                    {
                        PhotoId = photo.PhotoId,
                        UserId = photo.UserId,
                        Username = photo.Username,
                        AlbumId = photo.AlbumId,
                        AlbumTitle = photo.AlbumTitle,
                        Title = photo.Title,
                        Description = photo.Description,
                        ImageUrl = photo.ImageUrl,
                        UploadedAt = photo.UploadedAt,
                        Tags = tags.ToList(),
                        LikesCount = likesCount,
                        CommentsCount = commentsCount
                    });
                }

                return Ok(photoDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting photos by album: {AlbumId}", albumId);
                return StatusCode(500, "An error occurred while retrieving photos by album");
            }
        }

        /// <summary>
        /// Get users who liked a specific photo
        /// </summary>
        [HttpGet("{id}/likes/users")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsersWhoLikedPhoto(int id)
        {
            try
            {
                var photoExists = await _photoRepository.ExistsAsync(id);
                if (!photoExists)
                {
                    return NotFound("Photo not found");
                }

                var users = await _likeRepository.GetUsersWhoLikedPhotoAsync(id);
                
                var userDtos = users.Select(u => new UserDto
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    Email = u.Email,
                    FullName = u.FullName,
                    Bio = u.Bio,
                    ProfilePic = u.ProfilePic,
                    CreatedAt = u.CreatedAt
                }).ToList();

                return Ok(userDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users who liked photo: {PhotoId}", id);
                return StatusCode(500, "An error occurred while retrieving users who liked the photo");
            }
        }

        /// <summary>
        /// Get a photo by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<PhotoDto>> GetPhoto(int id)
        {
            try
            {
                var photo = await _photoRepository.GetByIdWithDetailsAsync(id);

                if (photo == null)
                {
                    return NotFound();
                }

                var tags = await _photoTagRepository.GetTagNamesByPhotoIdAsync(id);
                var likesCount = await _likeRepository.GetCountByPhotoIdAsync(id);
                var commentsCount = await _commentRepository.GetCountByPhotoIdAsync(id);

                var photoDto = new PhotoDto
                {
                    PhotoId = photo.PhotoId,
                    UserId = photo.UserId,
                    Username = photo.Username,
                    AlbumId = photo.AlbumId,
                    AlbumTitle = photo.AlbumTitle,
                    Title = photo.Title,
                    Description = photo.Description,
                    ImageUrl = photo.ImageUrl,
                    UploadedAt = photo.UploadedAt,
                    Tags = tags.ToList(),
                    LikesCount = likesCount,
                    CommentsCount = commentsCount
                };

                return Ok(photoDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting photo with ID: {PhotoId}", id);
                return StatusCode(500, "An error occurred while retrieving the photo");
            }
        }

        /// <summary>
        /// Get photos by user ID
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<PhotoDto>>> GetPhotosByUser(int userId)
        {
            try
            {
                var photos = await _photoRepository.GetByUserIdAsync(userId);
                
                var photoDtos = new List<PhotoDto>();
                foreach (var photo in photos)
                {
                    var tags = await _photoTagRepository.GetTagNamesByPhotoIdAsync(photo.PhotoId);
                    var likesCount = await _likeRepository.GetCountByPhotoIdAsync(photo.PhotoId);
                    var commentsCount = await _commentRepository.GetCountByPhotoIdAsync(photo.PhotoId);
                    
                    photoDtos.Add(new PhotoDto
                    {
                        PhotoId = photo.PhotoId,
                        UserId = photo.UserId,
                        Username = photo.Username,
                        AlbumId = photo.AlbumId,
                        AlbumTitle = photo.AlbumTitle,
                        Title = photo.Title,
                        Description = photo.Description,
                        ImageUrl = photo.ImageUrl,
                        UploadedAt = photo.UploadedAt,
                        Tags = tags.ToList(),
                        LikesCount = likesCount,
                        CommentsCount = commentsCount
                    });
                }

                return Ok(photoDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting photos by user: {UserId}", userId);
                return StatusCode(500, "An error occurred while retrieving photos by user");
            }
        }

        /// <summary>
        /// Create a new photo
        /// </summary>
        [HttpPost("user/{userId}")]
        public async Task<ActionResult<PhotoDto>> CreatePhoto(int userId, CreatePhotoDto createPhotoDto)
        {
            try
            {
                var userExists = await _userRepository.ExistsAsync(userId);
                if (!userExists)
                {
                    return BadRequest("User not found");
                }

                if (createPhotoDto.AlbumId.HasValue)
                {
                    var albumBelongsToUser = await _albumRepository.ExistsAndBelongsToUserAsync(createPhotoDto.AlbumId.Value, userId);
                    if (!albumBelongsToUser)
                    {
                        return BadRequest("Album not found or doesn't belong to user");
                    }
                }

                var photo = new Photo
                {
                    UserId = userId,
                    AlbumId = createPhotoDto.AlbumId,
                    Title = createPhotoDto.Title,
                    Description = createPhotoDto.Description,
                    ImageUrl = createPhotoDto.ImageUrl,
                    UploadedAt = DateTime.Now
                };

                var createdPhoto = await _photoRepository.CreateAsync(photo);

                // Add tags
                foreach (var tagName in createPhotoDto.Tags)
                {
                    var tag = await _tagRepository.GetByNameAsync(tagName);
                    if (tag == null)
                    {
                        tag = new Tag { TagName = tagName };
                        tag = await _tagRepository.CreateAsync(tag);
                    }

                    var photoTag = new PhotoTag
                    {
                        PhotoId = createdPhoto.PhotoId,
                        TagId = tag.TagId
                    };
                    await _photoTagRepository.CreateAsync(photoTag);
                }

                // Get the photo with details for response
                var photoWithDetails = await _photoRepository.GetByIdWithDetailsAsync(createdPhoto.PhotoId);
                if (photoWithDetails == null)
                {
                    _logger.LogError("Created photo not found after creation: {PhotoId}", createdPhoto.PhotoId);
                    return StatusCode(500, "An error occurred while retrieving the created photo");
                }

                var tags = await _photoTagRepository.GetTagNamesByPhotoIdAsync(createdPhoto.PhotoId);
                var likesCount = await _likeRepository.GetCountByPhotoIdAsync(createdPhoto.PhotoId);
                var commentsCount = await _commentRepository.GetCountByPhotoIdAsync(createdPhoto.PhotoId);

                var photoDto = new PhotoDto
                {
                    PhotoId = photoWithDetails.PhotoId,
                    UserId = photoWithDetails.UserId,
                    Username = photoWithDetails.Username,
                    AlbumId = photoWithDetails.AlbumId,
                    AlbumTitle = photoWithDetails.AlbumTitle,
                    Title = photoWithDetails.Title,
                    Description = photoWithDetails.Description,
                    ImageUrl = photoWithDetails.ImageUrl,
                    UploadedAt = photoWithDetails.UploadedAt,
                    Tags = tags.ToList(),
                    LikesCount = likesCount,
                    CommentsCount = commentsCount
                };

                return CreatedAtAction(nameof(GetPhoto), new { id = createdPhoto.PhotoId }, photoDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating photo for user: {UserId}", userId);
                return StatusCode(500, "An error occurred while creating the photo");
            }
        }

        /// <summary>
        /// Update a photo
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePhoto(int id, UpdatePhotoDto updatePhotoDto)
        {
            try
            {
                var photo = await _photoRepository.GetByIdAsync(id);
                if (photo == null)
                {
                    return NotFound();
                }

                photo.Title = updatePhotoDto.Title ?? photo.Title;
                photo.Description = updatePhotoDto.Description ?? photo.Description;
                photo.AlbumId = updatePhotoDto.AlbumId ?? photo.AlbumId;

                // Update the photo
                await _photoRepository.UpdateAsync(photo);

                // Update tags - remove all existing tags first
                await _photoTagRepository.DeleteAllByPhotoIdAsync(id);

                // Add new tags
                foreach (var tagName in updatePhotoDto.Tags)
                {
                    var tag = await _tagRepository.GetByNameAsync(tagName);
                    if (tag == null)
                    {
                        tag = new Tag { TagName = tagName };
                        tag = await _tagRepository.CreateAsync(tag);
                    }

                    var photoTag = new PhotoTag
                    {
                        PhotoId = photo.PhotoId,
                        TagId = tag.TagId
                    };
                    await _photoTagRepository.CreateAsync(photoTag);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating photo: {PhotoId}", id);
                return StatusCode(500, "An error occurred while updating the photo");
            }
        }

        /// <summary>
        /// Delete a photo
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int id)
        {
            try
            {
                var photoExists = await _photoRepository.ExistsAsync(id);
                if (!photoExists)
                {
                    return NotFound();
                }

                var deleted = await _photoRepository.DeleteAsync(id);
                if (!deleted)
                {
                    return StatusCode(500, "Failed to delete photo");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting photo: {PhotoId}", id);
                return StatusCode(500, "An error occurred while deleting the photo");
            }
        }
    }
}