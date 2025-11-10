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
    public class AlbumsController : ControllerBase
    {
        private readonly IAlbumRepository _albumRepository;
        private readonly IPhotoRepository _photoRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AlbumsController> _logger;

        public AlbumsController(IAlbumRepository albumRepository, IPhotoRepository photoRepository, IUserRepository userRepository, ILogger<AlbumsController> logger)
        {
            _albumRepository = albumRepository;
            _photoRepository = photoRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get all albums
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AlbumDto>>> GetAlbums()
        {
            var albums = await _albumRepository.GetAllAsync();
            
            var albumDtos = albums.Select(a => new AlbumDto
            {
                AlbumId = a.AlbumId,
                UserId = a.UserId,
                Username = a.Username,
                Title = a.Title ?? string.Empty,
                Description = a.Description,
                CreatedAt = a.CreatedAt,
                PhotosCount = a.PhotosCount
            }).ToList();

            return Ok(albumDtos);
        }

        /// <summary>
        /// Get an album by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<AlbumDto>> GetAlbum(int id)
        {
            var album = await _albumRepository.GetByIdWithDetailsAsync(id);

            if (album == null)
            {
                return NotFound();
            }

            var albumDto = new AlbumDto
            {
                AlbumId = album.AlbumId,
                UserId = album.UserId,
                Username = album.Username,
                Title = album.Title ?? string.Empty,
                Description = album.Description,
                CreatedAt = album.CreatedAt,
                PhotosCount = album.PhotosCount
            };

            return Ok(albumDto);
        }

        /// <summary>
        /// Get albums by user ID
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<AlbumDto>>> GetAlbumsByUser(int userId)
        {
            var albums = await _albumRepository.GetByUserIdAsync(userId);
            
            var albumDtos = albums.Select(a => new AlbumDto
            {
                AlbumId = a.AlbumId,
                UserId = a.UserId,
                Username = a.Username,
                Title = a.Title ?? string.Empty,
                Description = a.Description,
                CreatedAt = a.CreatedAt,
                PhotosCount = a.PhotosCount
            }).ToList();

            return Ok(albumDtos);
        }

        /// <summary>
        /// Create a new album
        /// </summary>
        [HttpPost("user/{userId}")]
        public async Task<ActionResult<AlbumDto>> CreateAlbum(int userId, CreateAlbumDto createAlbumDto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            var album = new Album
            {
                UserId = userId,
                Title = createAlbumDto.Title,
                Description = createAlbumDto.Description,
                CreatedAt = DateTime.Now
            };

            var createdAlbum = await _albumRepository.CreateAsync(album);

            var albumDto = new AlbumDto
            {
                AlbumId = createdAlbum.AlbumId,
                UserId = createdAlbum.UserId,
                Username = user.Username,
                Title = createdAlbum.Title,
                Description = createdAlbum.Description,
                CreatedAt = createdAlbum.CreatedAt,
                PhotosCount = 0
            };

            return CreatedAtAction(nameof(GetAlbum), new { id = album.AlbumId }, albumDto);
        }

        /// <summary>
        /// Update an album
        /// </summary>
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAlbum(int id, UpdateAlbumDto updateAlbumDto)
        {
            var album = await _albumRepository.GetByIdAsync(id);
            if (album == null)
            {
                return NotFound();
            }

            // Ensure the authenticated user owns this album
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var authUserId))
            {
                return Unauthorized("Invalid user token");
            }
            if (album.UserId != authUserId)
            {
                return Forbid();
            }

            album.Title = updateAlbumDto.Title ?? album.Title;
            album.Description = updateAlbumDto.Description ?? album.Description;

            await _albumRepository.UpdateAsync(album);

            return NoContent();
        }

        /// <summary>
        /// Delete an album
        /// </summary>
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAlbum(int id)
        {
            var albumExists = await _albumRepository.GetByIdAsync(id);
            if (albumExists == null)
            {
                return NotFound();
            }

            // Ensure the authenticated user owns this album
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var authUserId))
            {
                return Unauthorized("Invalid user token");
            }
            if (albumExists.UserId != authUserId)
            {
                return Forbid();
            }

            await _albumRepository.DeleteAsync(id);

            return NoContent();
        }

        /// <summary>
        /// Get photos in an album
        /// </summary>
        [HttpGet("{id}/photos")]
        public async Task<ActionResult<IEnumerable<PhotoDto>>> GetAlbumPhotos(int id)
        {
            var album = await _albumRepository.GetByIdAsync(id);
            if (album == null)
            {
                return NotFound();
            }

            var photos = await _photoRepository.GetByAlbumIdAsync(id);

            var photoDtos = photos.Select(p => new PhotoDto
            {
                PhotoId = p.PhotoId,
                UserId = p.UserId,
                Username = "", // Will need to be populated by a method that includes username
                AlbumId = p.AlbumId,
                AlbumTitle = album.Title,
                Title = p.Title,
                Description = p.Description,
                ImageUrl = p.ImageUrl,
                FullImageUrl = string.IsNullOrEmpty(p.ImageUrl) ? null : (p.ImageUrl.StartsWith("http") || p.ImageUrl.StartsWith("data:") ? p.ImageUrl : $"{Request.Scheme}://{Request.Host}/images/{Uri.EscapeDataString(p.ImageUrl)}"),
                UploadedAt = p.UploadedAt,
                Tags = new List<string>(), // Will need to be populated separately
                LikesCount = 0, // Will need to be populated separately
                CommentsCount = 0 // Will need to be populated separately
            }).ToList();

            return Ok(photoDtos);
        }
    }
}