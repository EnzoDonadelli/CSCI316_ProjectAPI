using Microsoft.AspNetCore.Mvc;

namespace VisaoAPI.Controllers
{
    [ApiController]
    [Route("images")]
    public class ImagesController : ControllerBase
    {
        // This endpoint will redirect to a placeholder image service (picsum) using the filename as a seed.
        // If you later add real image files under wwwroot/images, you can update this to serve files directly.
        [HttpGet("{fileName}")]
        public IActionResult GetImage(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return BadRequest();

            // sanitize filename: keep only letters, numbers, dash, underscore and dot
            var safe = new string(fileName.Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == '.').ToArray());

            // Use picsum.photos seeded by the filename (without extension) to provide a consistent placeholder image.
            var seed = System.IO.Path.GetFileNameWithoutExtension(safe);
            var url = $"https://picsum.photos/seed/{Uri.EscapeDataString(seed)}/1200/800";

            return Redirect(url);
        }
    }
}
