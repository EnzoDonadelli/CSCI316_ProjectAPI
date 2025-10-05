using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisaoAPI.Data;
using VisaoAPI.DTOs;
using VisaoAPI.Models;

namespace VisaoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlbumsController : ControllerBase
    {
        private readonly PhotoSharingDbContext _context;
        private readonly ILogger<AlbumsController> _logger;

        public AlbumsController(PhotoSharingDbContext context, ILogger<AlbumsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all albums
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AlbumDto>>> GetAlbums()
        {
            var albums = await _context.Albums
                .Include(a => a.User)
                .Include(a => a.Photos)
                .Select(a => new AlbumDto
                {
                    AlbumId = a.AlbumId,
                    UserId = a.UserId,
                    Username = a.User.Username,
                    Title = a.Title,
                    Description = a.Description,
                    CreatedAt = a.CreatedAt,
                    PhotosCount = a.Photos.Count
                })
                .ToListAsync();

            return Ok(albums);
        }

        /// <summary>
        /// Get an album by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<AlbumDto>> GetAlbum(int id)
        {
            var album = await _context.Albums
                .Include(a => a.User)
                .Include(a => a.Photos)
                .Where(a => a.AlbumId == id)
                .Select(a => new AlbumDto
                {
                    AlbumId = a.AlbumId,
                    UserId = a.UserId,
                    Username = a.User.Username,
                    Title = a.Title,
                    Description = a.Description,
                    CreatedAt = a.CreatedAt,
                    PhotosCount = a.Photos.Count
                })
                .FirstOrDefaultAsync();

            if (album == null)
            {
                return NotFound();
            }

            return Ok(album);
        }

        /// <summary>
        /// Get albums by user ID
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<AlbumDto>>> GetAlbumsByUser(int userId)
        {
            var albums = await _context.Albums
                .Where(a => a.UserId == userId)
                .Include(a => a.User)
                .Include(a => a.Photos)
                .Select(a => new AlbumDto
                {
                    AlbumId = a.AlbumId,
                    UserId = a.UserId,
                    Username = a.User.Username,
                    Title = a.Title,
                    Description = a.Description,
                    CreatedAt = a.CreatedAt,
                    PhotosCount = a.Photos.Count
                })
                .ToListAsync();

            return Ok(albums);
        }

        /// <summary>
        /// Create a new album
        /// </summary>
        [HttpPost("user/{userId}")]
        public async Task<ActionResult<AlbumDto>> CreateAlbum(int userId, CreateAlbumDto createAlbumDto)
        {
            var user = await _context.Users.FindAsync(userId);
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

            _context.Albums.Add(album);
            await _context.SaveChangesAsync();

            var albumDto = new AlbumDto
            {
                AlbumId = album.AlbumId,
                UserId = album.UserId,
                Username = user.Username,
                Title = album.Title,
                Description = album.Description,
                CreatedAt = album.CreatedAt,
                PhotosCount = 0
            };

            return CreatedAtAction(nameof(GetAlbum), new { id = album.AlbumId }, albumDto);
        }

        /// <summary>
        /// Update an album
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAlbum(int id, UpdateAlbumDto updateAlbumDto)
        {
            var album = await _context.Albums.FindAsync(id);
            if (album == null)
            {
                return NotFound();
            }

            album.Title = updateAlbumDto.Title ?? album.Title;
            album.Description = updateAlbumDto.Description ?? album.Description;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Delete an album
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAlbum(int id)
        {
            var album = await _context.Albums.FindAsync(id);
            if (album == null)
            {
                return NotFound();
            }

            _context.Albums.Remove(album);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Get photos in an album
        /// </summary>
        [HttpGet("{id}/photos")]
        public async Task<ActionResult<IEnumerable<PhotoDto>>> GetAlbumPhotos(int id)
        {
            var album = await _context.Albums.FindAsync(id);
            if (album == null)
            {
                return NotFound();
            }

            var photos = await _context.Photos
                .Where(p => p.AlbumId == id)
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
    }
}