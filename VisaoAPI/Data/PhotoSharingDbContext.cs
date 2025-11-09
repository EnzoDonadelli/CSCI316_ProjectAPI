using Microsoft.EntityFrameworkCore;
using VisaoAPI.Models;

namespace VisaoAPI.Data
{
    public class PhotoSharingDbContext : DbContext
    {
        public PhotoSharingDbContext(DbContextOptions<PhotoSharingDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Album> Albums { get; set; } = null!;
        public DbSet<Photo> Photos { get; set; } = null!;
        public DbSet<Tag> Tags { get; set; } = null!;
        public DbSet<PhotoTag> PhotoTags { get; set; } = null!;
        public DbSet<Like> Likes { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
        public DbSet<Follower> Followers { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            });

            // Album configuration
            modelBuilder.Entity<Album>(entity =>
            {
                entity.HasKey(e => e.AlbumId);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Albums)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Photo configuration
            modelBuilder.Entity<Photo>(entity =>
            {
                entity.HasKey(e => e.PhotoId);
                entity.Property(e => e.UploadedAt).HasDefaultValueSql("GETDATE()");
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Photos)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.Album)
                    .WithMany(a => a.Photos)
                    .HasForeignKey(e => e.AlbumId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Tag configuration
            modelBuilder.Entity<Tag>(entity =>
            {
                entity.HasKey(e => e.TagId);
                entity.HasIndex(e => e.TagName).IsUnique();
            });

            // PhotoTag configuration (many-to-many)
            modelBuilder.Entity<PhotoTag>(entity =>
            {
                entity.HasKey(e => new { e.PhotoId, e.TagId });
                entity.HasOne(e => e.Photo)
                    .WithMany(p => p.PhotoTags)
                    .HasForeignKey(e => e.PhotoId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.Tag)
                    .WithMany(t => t.PhotoTags)
                    .HasForeignKey(e => e.TagId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Like configuration
            modelBuilder.Entity<Like>(entity =>
            {
                entity.HasKey(e => e.LikeId);
                entity.HasIndex(e => new { e.UserId, e.PhotoId }).IsUnique();
                entity.Property(e => e.LikedAt).HasDefaultValueSql("GETDATE()");
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Likes)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.Photo)
                    .WithMany(p => p.Likes)
                    .HasForeignKey(e => e.PhotoId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Comment configuration
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(e => e.CommentId);
                entity.Property(e => e.CommentedAt).HasDefaultValueSql("GETDATE()");
                entity.HasOne(e => e.Photo)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(e => e.PhotoId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Comments)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Follower configuration (many-to-many self-referencing)
            modelBuilder.Entity<Follower>(entity =>
            {
                entity.HasKey(e => new { e.FollowerId, e.FollowingId });
                entity.Property(e => e.FollowedAt).HasDefaultValueSql("GETDATE()");
                entity.HasOne(e => e.FollowerUser)
                    .WithMany(u => u.Following)
                    .HasForeignKey(e => e.FollowerId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.FollowingUser)
                    .WithMany(u => u.Followers)
                    .HasForeignKey(e => e.FollowingId)
                    .OnDelete(DeleteBehavior.NoAction);
            });
        }
    }
}