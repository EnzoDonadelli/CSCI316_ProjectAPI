using Microsoft.EntityFrameworkCore;
using VisaoAPI.Data;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<PhotoSharingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Photo Sharing API",
        Version = "v1",
        Description = "A comprehensive API for photo sharing functionality including users, albums, photos, comments, and likes.",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "VisaoAPI Team"
        }
    });

    // Include XML comments for better documentation
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Add CORS policy for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Photo Sharing API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at the app's root
    });
    app.UseCors("AllowAll");
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Ensure database is created and seeded
try 
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<PhotoSharingDbContext>();
        
        // Ensure database is created
        context.Database.EnsureCreated();
        
        // Clear existing data and insert fresh sample data every time
        try
        {
            context.Database.ExecuteSqlRaw("DELETE FROM Comments");
            context.Database.ExecuteSqlRaw("DELETE FROM Likes");
            context.Database.ExecuteSqlRaw("DELETE FROM Followers");
            context.Database.ExecuteSqlRaw("DELETE FROM Photo_Tags");
            context.Database.ExecuteSqlRaw("DELETE FROM Photos");
            context.Database.ExecuteSqlRaw("DELETE FROM Albums");
            context.Database.ExecuteSqlRaw("DELETE FROM Tags");
            context.Database.ExecuteSqlRaw("DELETE FROM Users");
            
            // Reset identity columns
            context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('Users', RESEED, 0)");
            context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('Albums', RESEED, 0)");
            context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('Photos', RESEED, 0)");
            context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('Tags', RESEED, 0)");
            context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('Likes', RESEED, 0)");
            context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('Comments', RESEED, 0)");
        }
        catch
        {
            // Tables might not exist yet, ignore errors
        }
        
        // Always insert fresh sample data
        var user1 = new VisaoAPI.Models.User
        {
            Username = "johndoe",
            Email = "john@example.com",
            PasswordHash = "hashed_password_123",
            FullName = "John Doe",
            Bio = "Landscape photographer.",
            ProfilePic = "profile1.jpg",
            CreatedAt = DateTime.Now
        };

        var user2 = new VisaoAPI.Models.User
        {
            Username = "janesmith",
            Email = "jane@example.com",
            PasswordHash = "hashed_password_456",
            FullName = "Jane Smith",
            Bio = "Wedding photographer.",
            ProfilePic = "profile2.jpg",
            CreatedAt = DateTime.Now
        };

        context.Users.AddRange(user1, user2);
        context.SaveChanges();

        var album1 = new VisaoAPI.Models.Album
        {
            UserId = user1.UserId,
            Title = "Nature Escapes",
            Description = "Collection of stunning landscape shots.",
            CreatedAt = DateTime.Now
        };

        var album2 = new VisaoAPI.Models.Album
        {
            UserId = user2.UserId,
            Title = "Forever Moments",
            Description = "Beautiful wedding memories.",
            CreatedAt = DateTime.Now
        };

        context.Albums.AddRange(album1, album2);
        context.SaveChanges();

        var photos = new[]
        {
            new VisaoAPI.Models.Photo
            {
                UserId = user1.UserId,
                AlbumId = album1.AlbumId,
                Title = "Mountain Sunrise",
                Description = "Sunrise over the peaks.",
                ImageUrl = "mountain_sunrise.jpg",
                UploadedAt = DateTime.Now
            },
            new VisaoAPI.Models.Photo
            {
                UserId = user1.UserId,
                AlbumId = album1.AlbumId,
                Title = "Forest Stream",
                Description = "Calm stream running through forest.",
                ImageUrl = "forest_stream.jpg",
                UploadedAt = DateTime.Now
            },
            new VisaoAPI.Models.Photo
            {
                UserId = user2.UserId,
                AlbumId = album2.AlbumId,
                Title = "Wedding Kiss",
                Description = "Couple sharing their first kiss.",
                ImageUrl = "wedding_kiss.jpg",
                UploadedAt = DateTime.Now
            },
            new VisaoAPI.Models.Photo
            {
                UserId = user2.UserId,
                AlbumId = album2.AlbumId,
                Title = "Reception Fun",
                Description = "Guests enjoying the reception.",
                ImageUrl = "reception_fun.jpg",
                UploadedAt = DateTime.Now
            }
        };

        context.Photos.AddRange(photos);
        context.SaveChanges();

        var tag1 = new VisaoAPI.Models.Tag { TagName = "Landscape" };
        var tag2 = new VisaoAPI.Models.Tag { TagName = "Wedding" };
        context.Tags.AddRange(tag1, tag2);
        context.SaveChanges();

        var photoTags = new[]
        {
            new VisaoAPI.Models.PhotoTag { PhotoId = photos[0].PhotoId, TagId = tag1.TagId },
            new VisaoAPI.Models.PhotoTag { PhotoId = photos[1].PhotoId, TagId = tag1.TagId },
            new VisaoAPI.Models.PhotoTag { PhotoId = photos[2].PhotoId, TagId = tag2.TagId },
            new VisaoAPI.Models.PhotoTag { PhotoId = photos[3].PhotoId, TagId = tag2.TagId }
        };
        context.PhotoTags.AddRange(photoTags);
        context.SaveChanges();

        var followers = new[]
        {
            new VisaoAPI.Models.Follower { FollowerId = user1.UserId, FollowingId = user2.UserId, FollowedAt = DateTime.Now },
            new VisaoAPI.Models.Follower { FollowerId = user2.UserId, FollowingId = user1.UserId, FollowedAt = DateTime.Now }
        };
        context.Followers.AddRange(followers);
        context.SaveChanges();

        var likes = new[]
        {
            new VisaoAPI.Models.Like { UserId = user1.UserId, PhotoId = photos[2].PhotoId, LikedAt = DateTime.Now },
            new VisaoAPI.Models.Like { UserId = user1.UserId, PhotoId = photos[3].PhotoId, LikedAt = DateTime.Now },
            new VisaoAPI.Models.Like { UserId = user2.UserId, PhotoId = photos[0].PhotoId, LikedAt = DateTime.Now },
            new VisaoAPI.Models.Like { UserId = user2.UserId, PhotoId = photos[1].PhotoId, LikedAt = DateTime.Now }
        };
        context.Likes.AddRange(likes);
        context.SaveChanges();

        var comments = new[]
        {
            new VisaoAPI.Models.Comment { PhotoId = photos[2].PhotoId, UserId = user1.UserId, CommentText = "Great job! Beautiful moment.", CommentedAt = DateTime.Now },
            new VisaoAPI.Models.Comment { PhotoId = photos[3].PhotoId, UserId = user1.UserId, CommentText = "Good view! Love the lighting.", CommentedAt = DateTime.Now },
            new VisaoAPI.Models.Comment { PhotoId = photos[0].PhotoId, UserId = user2.UserId, CommentText = "Amazing capture! Great job!", CommentedAt = DateTime.Now },
            new VisaoAPI.Models.Comment { PhotoId = photos[1].PhotoId, UserId = user2.UserId, CommentText = "Beautiful scenery!", CommentedAt = DateTime.Now }
        };
        context.Comments.AddRange(comments);
        context.SaveChanges();

        Console.WriteLine("✅ Fresh sample data created successfully!");
        Console.WriteLine($"✅ Created {context.Users.Count()} users");
        Console.WriteLine($"✅ Created {context.Photos.Count()} photos");
        Console.WriteLine($"✅ Created {context.Albums.Count()} albums");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error creating sample data: {ex.Message}");
}

app.Run();
