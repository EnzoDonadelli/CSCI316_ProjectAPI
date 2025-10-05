using Microsoft.EntityFrameworkCore;
using System.Data;
using VisaoAPI.Data;

namespace VisaoAPI.Services
{
    public class DatabaseSeeder
    {
        private readonly PhotoSharingDbContext _context;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(PhotoSharingDbContext context, ILogger<DatabaseSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                // Check if data already exists
                if (await _context.Users.AnyAsync())
                {
                    _logger.LogInformation("Database already contains data. Skipping seeding.");
                    return;
                }

                _logger.LogInformation("Starting database seeding...");

                // Read and execute the SQL file
                var sqlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "SampleData.sql");
                
                if (File.Exists(sqlFilePath))
                {
                    var sqlScript = await File.ReadAllTextAsync(sqlFilePath);
                    
                    // Split the SQL script into individual commands
                    var commands = sqlScript.Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var command in commands)
                    {
                        var trimmedCommand = command.Trim();
                        if (!string.IsNullOrEmpty(trimmedCommand) && !trimmedCommand.StartsWith("--"))
                        {
                            await _context.Database.ExecuteSqlRawAsync(trimmedCommand);
                        }
                    }

                    _logger.LogInformation("Database seeding completed successfully!");
                }
                else
                {
                    _logger.LogWarning("SampleData.sql file not found. Creating sample data programmatically...");
                    await SeedDataProgrammatically();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database.");
                // Fallback to programmatic seeding
                await SeedDataProgrammatically();
            }
        }

        private async Task SeedDataProgrammatically()
        {
            _logger.LogInformation("Creating sample data programmatically...");

            // Create Users
            var user1 = new Models.User
            {
                Username = "johndoe",
                Email = "john@example.com",
                PasswordHash = "hashed_password_123",
                FullName = "John Doe",
                Bio = "Landscape photographer.",
                ProfilePic = "profile1.jpg",
                CreatedAt = DateTime.Now
            };

            var user2 = new Models.User
            {
                Username = "janesmith",
                Email = "jane@example.com",
                PasswordHash = "hashed_password_456",
                FullName = "Jane Smith",
                Bio = "Wedding photographer.",
                ProfilePic = "profile2.jpg",
                CreatedAt = DateTime.Now
            };

            _context.Users.AddRange(user1, user2);
            await _context.SaveChangesAsync();

            // Create Albums
            var album1 = new Models.Album
            {
                UserId = user1.UserId,
                Title = "Nature Escapes",
                Description = "Collection of stunning landscape shots.",
                CreatedAt = DateTime.Now
            };

            var album2 = new Models.Album
            {
                UserId = user2.UserId,
                Title = "Forever Moments",
                Description = "Beautiful wedding memories.",
                CreatedAt = DateTime.Now
            };

            _context.Albums.AddRange(album1, album2);
            await _context.SaveChangesAsync();

            // Create Photos
            var photo1 = new Models.Photo
            {
                UserId = user1.UserId,
                AlbumId = album1.AlbumId,
                Title = "Mountain Sunrise",
                Description = "Sunrise over the peaks.",
                ImageUrl = "mountain_sunrise.jpg",
                UploadedAt = DateTime.Now
            };

            var photo2 = new Models.Photo
            {
                UserId = user1.UserId,
                AlbumId = album1.AlbumId,
                Title = "Forest Stream",
                Description = "Calm stream running through forest.",
                ImageUrl = "forest_stream.jpg",
                UploadedAt = DateTime.Now
            };

            var photo3 = new Models.Photo
            {
                UserId = user2.UserId,
                AlbumId = album2.AlbumId,
                Title = "Wedding Kiss",
                Description = "Couple sharing their first kiss.",
                ImageUrl = "wedding_kiss.jpg",
                UploadedAt = DateTime.Now
            };

            var photo4 = new Models.Photo
            {
                UserId = user2.UserId,
                AlbumId = album2.AlbumId,
                Title = "Reception Fun",
                Description = "Guests enjoying the reception.",
                ImageUrl = "reception_fun.jpg",
                UploadedAt = DateTime.Now
            };

            _context.Photos.AddRange(photo1, photo2, photo3, photo4);
            await _context.SaveChangesAsync();

            // Create Tags
            var tag1 = new Models.Tag { TagName = "Landscape" };
            var tag2 = new Models.Tag { TagName = "Wedding" };

            _context.Tags.AddRange(tag1, tag2);
            await _context.SaveChangesAsync();

            // Create PhotoTags
            var photoTags = new[]
            {
                new Models.PhotoTag { PhotoId = photo1.PhotoId, TagId = tag1.TagId },
                new Models.PhotoTag { PhotoId = photo2.PhotoId, TagId = tag1.TagId },
                new Models.PhotoTag { PhotoId = photo3.PhotoId, TagId = tag2.TagId },
                new Models.PhotoTag { PhotoId = photo4.PhotoId, TagId = tag2.TagId }
            };

            _context.PhotoTags.AddRange(photoTags);
            await _context.SaveChangesAsync();

            // Create Followers
            var followers = new[]
            {
                new Models.Follower { FollowerId = user1.UserId, FollowingId = user2.UserId, FollowedAt = DateTime.Now },
                new Models.Follower { FollowerId = user2.UserId, FollowingId = user1.UserId, FollowedAt = DateTime.Now }
            };

            _context.Followers.AddRange(followers);
            await _context.SaveChangesAsync();

            // Create Likes
            var likes = new[]
            {
                new Models.Like { UserId = user1.UserId, PhotoId = photo3.PhotoId, LikedAt = DateTime.Now },
                new Models.Like { UserId = user1.UserId, PhotoId = photo4.PhotoId, LikedAt = DateTime.Now },
                new Models.Like { UserId = user2.UserId, PhotoId = photo1.PhotoId, LikedAt = DateTime.Now },
                new Models.Like { UserId = user2.UserId, PhotoId = photo2.PhotoId, LikedAt = DateTime.Now }
            };

            _context.Likes.AddRange(likes);
            await _context.SaveChangesAsync();

            // Create Comments
            var comments = new[]
            {
                new Models.Comment { PhotoId = photo3.PhotoId, UserId = user1.UserId, CommentText = "Great job! Beautiful moment.", CommentedAt = DateTime.Now },
                new Models.Comment { PhotoId = photo4.PhotoId, UserId = user1.UserId, CommentText = "Good view! Love the lighting.", CommentedAt = DateTime.Now },
                new Models.Comment { PhotoId = photo1.PhotoId, UserId = user2.UserId, CommentText = "Amazing capture! Great job!", CommentedAt = DateTime.Now },
                new Models.Comment { PhotoId = photo2.PhotoId, UserId = user2.UserId, CommentText = "Beautiful scenery!", CommentedAt = DateTime.Now }
            };

            _context.Comments.AddRange(comments);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Programmatic database seeding completed successfully!");
        }
    }
}