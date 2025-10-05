using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisaoAPI.Data;
using VisaoAPI.DTOs;
using VisaoAPI.Models;

namespace VisaoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PhotosController : ControllerBase
    {
        private readonly PhotoSharingDbContext _context;
        private readonly ILogger<PhotosController> _logger;

        public PhotosController(PhotoSharingDbContext context, ILogger<PhotosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all photos
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PhotoDto>>> GetPhotos()
        {
            var photos = await _context.Photos
                .Include(p => p.User)
                .Include(p => p.Album)
                .Include(p => p.PhotoTags)
                .ThenInclude(pt => pt.Tag)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .Select(p => new PhotoDto
                {
                    PhotoId = p.PhotoId,
                    UserId = p.UserId,
                    Username = p.User.Username,
                    AlbumId = p.AlbumId,
                    AlbumTitle = p.Album != null ? p.Album.Title : null,
                    Title = p.Title,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    UploadedAt = p.UploadedAt,
                    Tags = p.PhotoTags.Select(pt => pt.Tag.TagName).ToList(),
                    LikesCount = p.Likes.Count,
                    CommentsCount = p.Comments.Count
                })
                .ToListAsync();

            return Ok(photos);
        }

        /// <summary>
        /// Get a photo by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<PhotoDto>> GetPhoto(int id)
        {
            var photo = await _context.Photos
                .Include(p => p.User)
                .Include(p => p.Album)
                .Include(p => p.PhotoTags)
                .ThenInclude(pt => pt.Tag)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .Where(p => p.PhotoId == id)
                .Select(p => new PhotoDto
                {
                    PhotoId = p.PhotoId,
                    UserId = p.UserId,
                    Username = p.User.Username,
                    AlbumId = p.AlbumId,
                    AlbumTitle = p.Album != null ? p.Album.Title : null,
                    Title = p.Title,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    UploadedAt = p.UploadedAt,
                    Tags = p.PhotoTags.Select(pt => pt.Tag.TagName).ToList(),
                    LikesCount = p.Likes.Count,
                    CommentsCount = p.Comments.Count
                })
                .FirstOrDefaultAsync();

            if (photo == null)
            {
                return NotFound();
            }

            return Ok(photo);
        }

        /// <summary>
        /// Get photos by user ID
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<PhotoDto>>> GetPhotosByUser(int userId)
        {
            var photos = await _context.Photos
                .Where(p => p.UserId == userId)
                .Include(p => p.User)
                .Include(p => p.Album)
                .Include(p => p.PhotoTags)
                .ThenInclude(pt => pt.Tag)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .Select(p => new PhotoDto
                {
                    PhotoId = p.PhotoId,
                    UserId = p.UserId,
                    Username = p.User.Username,
                    AlbumId = p.AlbumId,
                    AlbumTitle = p.Album != null ? p.Album.Title : null,
                    Title = p.Title,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    UploadedAt = p.UploadedAt,
                    Tags = p.PhotoTags.Select(pt => pt.Tag.TagName).ToList(),
                    LikesCount = p.Likes.Count,
                    CommentsCount = p.Comments.Count
                })
                .ToListAsync();

            return Ok(photos);
        }

        /// <summary>
        /// Create a new photo
        /// </summary>
        [HttpPost("user/{userId}")]
        public async Task<ActionResult<PhotoDto>> CreatePhoto(int userId, CreatePhotoDto createPhotoDto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            if (createPhotoDto.AlbumId.HasValue)
            {
                var album = await _context.Albums
                    .Where(a => a.AlbumId == createPhotoDto.AlbumId && a.UserId == userId)
                    .FirstOrDefaultAsync();
                if (album == null)
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

            _context.Photos.Add(photo);
            await _context.SaveChangesAsync();

            // Add tags
            foreach (var tagName in createPhotoDto.Tags)
            {
                var tag = await _context.Tags.FirstOrDefaultAsync(t => t.TagName == tagName);
                if (tag == null)
                {
                    tag = new Tag { TagName = tagName };
                    _context.Tags.Add(tag);
                    await _context.SaveChangesAsync();
                }

                var photoTag = new PhotoTag
                {
                    PhotoId = photo.PhotoId,
                    TagId = tag.TagId
                };
                _context.PhotoTags.Add(photoTag);
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPhoto), new { id = photo.PhotoId }, await GetPhotoDto(photo.PhotoId));
        }

        /// <summary>
        /// Update a photo
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePhoto(int id, UpdatePhotoDto updatePhotoDto)
        {
            var photo = await _context.Photos
                .Include(p => p.PhotoTags)
                .FirstOrDefaultAsync(p => p.PhotoId == id);

            if (photo == null)
            {
                return NotFound();
            }

            photo.Title = updatePhotoDto.Title ?? photo.Title;
            photo.Description = updatePhotoDto.Description ?? photo.Description;
            photo.AlbumId = updatePhotoDto.AlbumId ?? photo.AlbumId;

            // Update tags
            _context.PhotoTags.RemoveRange(photo.PhotoTags);
            foreach (var tagName in updatePhotoDto.Tags)
            {
                var tag = await _context.Tags.FirstOrDefaultAsync(t => t.TagName == tagName);
                if (tag == null)
                {
                    tag = new Tag { TagName = tagName };
                    _context.Tags.Add(tag);
                    await _context.SaveChangesAsync();
                }

                var photoTag = new PhotoTag
                {
                    PhotoId = photo.PhotoId,
                    TagId = tag.TagId
                };
                _context.PhotoTags.Add(photoTag);
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Delete a photo
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int id)
        {
            var photo = await _context.Photos.FindAsync(id);
            if (photo == null)
            {
                return NotFound();
            }

            _context.Photos.Remove(photo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<PhotoDto> GetPhotoDto(int photoId)
        {
            return await _context.Photos
                .Include(p => p.User)
                .Include(p => p.Album)
                .Include(p => p.PhotoTags)
                .ThenInclude(pt => pt.Tag)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .Where(p => p.PhotoId == photoId)
                .Select(p => new PhotoDto
                {
                    PhotoId = p.PhotoId,
                    UserId = p.UserId,
                    Username = p.User.Username,
                    AlbumId = p.AlbumId,
                    AlbumTitle = p.Album != null ? p.Album.Title : null,
                    Title = p.Title,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    UploadedAt = p.UploadedAt,
                    Tags = p.PhotoTags.Select(pt => pt.Tag.TagName).ToList(),
                    LikesCount = p.Likes.Count,
                    CommentsCount = p.Comments.Count
                })
                .FirstAsync();
        }
    }
}