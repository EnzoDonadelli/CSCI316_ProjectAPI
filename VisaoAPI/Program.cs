using Microsoft.EntityFrameworkCore;
using VisaoAPI.Data;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using VisaoAPI.Repositories;
using VisaoAPI.Services;
using BCrypt.Net;
using Microsoft.Extensions.FileProviders;

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
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<ILikeRepository, LikeRepository>();
builder.Services.AddScoped<IFollowerRepository, FollowerRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<IPhotoTagRepository, PhotoTagRepository>();
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

            // Serve images directly from the repository EXTRAS folder (preferred)
            // Fallback: if EXTRAS doesn't exist, serve any files in wwwroot/images
            var extrasPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "EXTRAS"));
            var imagesDir = Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "images");
            try
            {
                if (Directory.Exists(extrasPath))
                {
                    // Serve EXTRAS at /extras and also at /images for backwards-compatibility
                    app.UseStaticFiles(new StaticFileOptions
                    {
                        FileProvider = new PhysicalFileProvider(extrasPath),
                        RequestPath = "/extras"
                    });
                    app.UseStaticFiles(new StaticFileOptions
                    {
                        FileProvider = new PhysicalFileProvider(extrasPath),
                        RequestPath = "/images"
                    });
                    Console.WriteLine($"✅ Serving EXTRAS at /extras and /images from '{extrasPath}'");
                }
                else
                {
                    // Ensure a local images folder exists under wwwroot and serve it
                    Directory.CreateDirectory(imagesDir);
                    if (Directory.Exists(imagesDir))
                    {
                        app.UseStaticFiles(new StaticFileOptions
                        {
                            FileProvider = new PhysicalFileProvider(imagesDir),
                            RequestPath = "/images"
                        });
                        Console.WriteLine($"⚠️ EXTRAS not found; serving images from '{imagesDir}' instead");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Could not configure images static files: {ex.Message}");
            }

            // Ensure the three users exist (create them if needed)
            var userJohn = context.Users.SingleOrDefault(u => u.Username == "johndoe");
            if (userJohn == null)
            {
                userJohn = new VisaoAPI.Models.User
                {
                    Username = "johndoe",
                    Email = "john@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    FullName = "John Doe",
                    Bio = "Birdwatcher and landscape photographer.",
                    ProfilePic = "BIRDS (1).jpg",
                    CreatedAt = DateTime.Now
                };
                context.Users.Add(userJohn);
                context.SaveChanges();
            }

            var userJane = context.Users.SingleOrDefault(u => u.Username == "janesmith");
            if (userJane == null)
            {
                userJane = new VisaoAPI.Models.User
                {
                    Username = "janesmith",
                    Email = "jane@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password456"),
                    FullName = "Jane Smith",
                    Bio = "Architectural photographer.",
                    ProfilePic = "WINDOW (1).jpg",
                    CreatedAt = DateTime.Now
                };
                context.Users.Add(userJane);
                context.SaveChanges();
            }

            var userEnzo = context.Users.SingleOrDefault(u => u.Username == "enzodonadelli");
            if (userEnzo == null)
            {
                userEnzo = new VisaoAPI.Models.User
                {
                    Username = "enzodonadelli",
                    Email = "enzo@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password789"),
                    FullName = "Enzo Donadelli",
                    Bio = "Mountains and snow specialist.",
                    ProfilePic = "MOUNTAINS_ (1).jpg",
                    CreatedAt = DateTime.Now
                };
                context.Users.Add(userEnzo);
                context.SaveChanges();
            }

            // If we found the EXTRAS folder, import files; otherwise skip gracefully
            if (Directory.Exists(extrasPath))
            {
                // Build lists from EXTRAS filenames
                string[] birdFiles = Directory.GetFiles(extrasPath, "BIRDS*.jpg").Select(f => Path.GetFileName(f) ?? string.Empty).Where(n => !string.IsNullOrEmpty(n)).ToArray();
                string[] mountainFiles = Directory.GetFiles(extrasPath, "MOUNTAINS_*.jpg").Select(f => Path.GetFileName(f) ?? string.Empty).Where(n => !string.IsNullOrEmpty(n)).ToArray();
                string[] snowFiles = Directory.GetFiles(extrasPath, "SNOW*.jpg").Select(f => Path.GetFileName(f) ?? string.Empty).Where(n => !string.IsNullOrEmpty(n)).ToArray();
                string[] windowFiles = Directory.GetFiles(extrasPath, "WINDOW*.jpg").Select(f => Path.GetFileName(f) ?? string.Empty).Where(n => !string.IsNullOrEmpty(n)).ToArray();

                // Ensure albums exist and add photos if they are not already present
                Func<int, string, string, int> ensureAlbum = (userId, title, description) =>
                {
                    var alb = context.Albums.SingleOrDefault(a => a.UserId == userId && a.Title == title);
                    if (alb != null) return alb.AlbumId;
                    alb = new VisaoAPI.Models.Album { UserId = userId, Title = title, Description = description, CreatedAt = DateTime.Now };
                    context.Albums.Add(alb);
                    context.SaveChanges();
                    return alb.AlbumId;
                };

                var albumJohnId = ensureAlbum(userJohn.UserId, "Birds", "Bird photos from John's collection.");
                var albumEnzoId = ensureAlbum(userEnzo.UserId, "Snow & Mountains", "Enzo's mountain and snow photos.");
                var albumJaneId = ensureAlbum(userJane.UserId, "Window", "Window series by Jane.");

                // Add photos if missing
                void AddPhotosForFiles(string[] files, int userId, int albumId)
                {
                    foreach (var f in files)
                    {
                        if (context.Photos.Any(p => p.ImageUrl == f)) continue;
                        var photo = new VisaoAPI.Models.Photo
                        {
                            UserId = userId,
                            AlbumId = albumId,
                            Title = Path.GetFileNameWithoutExtension(f),
                            Description = $"Imported photo {f}",
                            ImageUrl = f,
                            UploadedAt = DateTime.Now
                        };
                        context.Photos.Add(photo);
                    }
                    context.SaveChanges();
                }

                AddPhotosForFiles(birdFiles, userJohn.UserId, albumJohnId);
                AddPhotosForFiles(mountainFiles.Concat(snowFiles).ToArray(), userEnzo.UserId, albumEnzoId);
                AddPhotosForFiles(windowFiles, userJane.UserId, albumJaneId);

                // Create tags from filenames (cleaned) and link to photos if not linked
                var allPhotos = context.Photos.ToList();
                var createdTags = context.Tags.ToDictionary(t => t.TagName, StringComparer.OrdinalIgnoreCase);

                foreach (var p in allPhotos)
                {
                    if (string.IsNullOrEmpty(p.ImageUrl)) continue;
                    var raw = Path.GetFileNameWithoutExtension(p.ImageUrl);
                    var cleaned = System.Text.RegularExpressions.Regex.Replace(raw ?? string.Empty, "\\s*\\(.*\\)", "");
                    cleaned = cleaned.Replace('_', ' ').Trim();
                    var tagName = cleaned.Split(' ')[0];
                    if (string.IsNullOrWhiteSpace(tagName)) continue;

                    if (!createdTags.TryGetValue(tagName, out var tag))
                    {
                        tag = new VisaoAPI.Models.Tag { TagName = tagName };
                        context.Tags.Add(tag);
                        context.SaveChanges();
                        createdTags[tag.TagName] = tag;
                    }

                    if (!context.PhotoTags.Any(pt => pt.PhotoId == p.PhotoId && pt.TagId == tag.TagId))
                    {
                        context.PhotoTags.Add(new VisaoAPI.Models.PhotoTag { PhotoId = p.PhotoId, TagId = tag.TagId });
                    }
                }
                context.SaveChanges();

                Console.WriteLine("✅ EXTRAS images imported and linked to database (users/albums/tags)");
            }
            else
            {
                Console.WriteLine($"⚠️ EXTRAS folder not found at '{extrasPath}' - skipping image import");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error creating sample data: {ex.Message}");
    }

app.Run();
