using Microsoft.EntityFrameworkCore;
using VisaoAPI.Data;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using VisaoAPI.Repositories;
using VisaoAPI.Services;
using BCrypt.Net;

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

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            },
            new List<string>()
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

// Add JWT Authentication
var jwtSecretKey = builder.Configuration["JwtSettings:SecretKey"];
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"];
var jwtAudience = builder.Configuration["JwtSettings:Audience"];

if (string.IsNullOrEmpty(jwtSecretKey))
{
    throw new InvalidOperationException("JWT SecretKey is not configured in appsettings.json");
}
if (string.IsNullOrEmpty(jwtIssuer))
{
    throw new InvalidOperationException("JWT Issuer is not configured in appsettings.json");
}
if (string.IsNullOrEmpty(jwtAudience))
{
    throw new InvalidOperationException("JWT Audience is not configured in appsettings.json");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey))
        };
    });

// Register dependencies
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPhotoRepository, PhotoRepository>();
builder.Services.AddScoped<IAlbumRepository, AlbumRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Ensure database is created and seeded
try 
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<PhotoSharingDbContext>();
        
        // Ensure database exists (but don't drop existing data)
        context.Database.EnsureCreated();
        
        // Only seed sample data if no users exist
        if (!context.Users.Any())
        {
            Console.WriteLine("🌱 Seeding initial sample data...");
            
            // Insert initial sample data with properly hashed passwords
        var user1 = new VisaoAPI.Models.User
        {
            Username = "johndoe",
            Email = "john@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), // Password: password123
            FullName = "John Doe",
            Bio = "Landscape photographer.",
            ProfilePic = "profile1.jpg",
            CreatedAt = DateTime.Now
        };

        var user2 = new VisaoAPI.Models.User
        {
            Username = "janesmith",
            Email = "jane@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password456"), // Password: password456
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

            Console.WriteLine("✅ Initial sample data created successfully!");
            Console.WriteLine($"✅ Created {context.Users.Count()} users");
            Console.WriteLine($"✅ Created {context.Photos.Count()} photos");
            Console.WriteLine($"✅ Created {context.Albums.Count()} albums");
        }
        else
        {
            Console.WriteLine($"📊 Database already contains {context.Users.Count()} users - skipping sample data");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error creating sample data: {ex.Message}");
}

app.Run();
