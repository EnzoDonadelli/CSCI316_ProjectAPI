using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisaoAPI.Data;
using VisaoAPI.Models;

namespace VisaoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TagsController : ControllerBase
    {
        private readonly PhotoSharingDbContext _context;
        private readonly ILogger<TagsController> _logger;

        public TagsController(PhotoSharingDbContext context, ILogger<TagsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all tags
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tag>>> GetTags()
        {
            var tags = await _context.Tags.OrderBy(t => t.TagName).ToListAsync();
            return Ok(tags);
        }

        /// <summary>
        /// Get photos by tag
        /// </summary>
        [HttpGet("{tagName}/photos")]
        public async Task<ActionResult<IEnumerable<object>>> GetPhotosByTag(string tagName)
        {
            var photos = await _context.PhotoTags
                .Where(pt => pt.Tag.TagName == tagName)
                .Include(pt => pt.Photo)
                .ThenInclude(p => p.User)
                .Include(pt => pt.Photo)
                .ThenInclude(p => p.Album)
                .Include(pt => pt.Photo)
                .ThenInclude(p => p.Likes)
                .Include(pt => pt.Photo)
                .ThenInclude(p => p.Comments)
                .Select(pt => new
                {
                    PhotoId = pt.Photo.PhotoId,
                    UserId = pt.Photo.UserId,
                    Username = pt.Photo.User.Username,
                    AlbumId = pt.Photo.AlbumId,
                    AlbumTitle = pt.Photo.Album != null ? pt.Photo.Album.Title : null,
                    Title = pt.Photo.Title,
                    Description = pt.Photo.Description,
                    ImageUrl = pt.Photo.ImageUrl,
                    UploadedAt = pt.Photo.UploadedAt,
                    LikesCount = pt.Photo.Likes.Count,
                    CommentsCount = pt.Photo.Comments.Count
                })
                .ToListAsync();

            return Ok(photos);
        }

        /// <summary>
        /// Get popular tags (most used)
        /// </summary>
        [HttpGet("popular")]
        public async Task<ActionResult<IEnumerable<object>>> GetPopularTags([FromQuery] int limit = 10)
        {
            var popularTags = await _context.Tags
                .Include(t => t.PhotoTags)
                .Select(t => new
                {
                    TagId = t.TagId,
                    TagName = t.TagName,
                    UsageCount = t.PhotoTags.Count
                })
                .OrderByDescending(t => t.UsageCount)
                .Take(limit)
                .ToListAsync();

            return Ok(popularTags);
        }
    }
}