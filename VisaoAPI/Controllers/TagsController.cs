using Microsoft.AspNetCore.Mvc;
using VisaoAPI.Models;
using VisaoAPI.Repositories;

namespace VisaoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TagsController : ControllerBase
    {
        private readonly ITagRepository _tagRepository;
        private readonly IPhotoTagRepository _photoTagRepository;
        private readonly ILogger<TagsController> _logger;

        public TagsController(
            ITagRepository tagRepository,
            IPhotoTagRepository photoTagRepository,
            ILogger<TagsController> logger)
        {
            _tagRepository = tagRepository;
            _photoTagRepository = photoTagRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get all tags
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tag>>> GetTags()
        {
            try
            {
                var tags = await _tagRepository.GetAllAsync();
                return Ok(tags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all tags");
                return StatusCode(500, "An error occurred while retrieving tags");
            }
        }

        /// <summary>
        /// Get photos by tag
        /// </summary>
        [HttpGet("{tagName}/photos")]
        public async Task<ActionResult<IEnumerable<object>>> GetPhotosByTag(string tagName)
        {
            try
            {
                var photos = await _photoTagRepository.GetPhotosByTagNameAsync(tagName, 50);

                var photoDtos = photos.Select(p => new
                {
                    PhotoId = p.PhotoId,
                    UserId = p.UserId,
                    Username = p.Username,
                    AlbumId = p.AlbumId,
                    AlbumTitle = p.AlbumTitle,
                    Title = p.Title,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    // Build a FullImageUrl like other controllers so the client can prefer it
                    FullImageUrl = string.IsNullOrEmpty(p.ImageUrl) ? null : (p.ImageUrl.StartsWith("http") || p.ImageUrl.StartsWith("data:") ? p.ImageUrl : $"{Request.Scheme}://{Request.Host}/images/{Uri.EscapeDataString(p.ImageUrl)}"),
                    UploadedAt = p.UploadedAt
                }).ToList();

                return Ok(photoDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting photos by tag: {TagName}", tagName);
                return StatusCode(500, "An error occurred while retrieving photos by tag");
            }
        }

        /// <summary>
        /// Get popular tags (most used)
        /// </summary>
        [HttpGet("popular")]
        public async Task<ActionResult<IEnumerable<object>>> GetPopularTags([FromQuery] int limit = 10)
        {
            try
            {
                var popularTags = await _tagRepository.GetPopularTagsWithUsageAsync(limit);

                var popularTagDtos = popularTags.Select(t => new
                {
                    TagId = t.TagId,
                    TagName = t.TagName,
                    UsageCount = t.UsageCount
                }).ToList();

                return Ok(popularTagDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting popular tags");
                return StatusCode(500, "An error occurred while retrieving popular tags");
            }
        }
    }
}