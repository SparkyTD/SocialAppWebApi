using Microsoft.EntityFrameworkCore;

namespace SocialAppWebApi.Data;

public class AppDatabase(DbContextOptions<AppDatabase> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<PostLike> PostLikes { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Username must be unique
        builder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();
        
        // Entity relationships
        builder.Entity<User>()
            .HasMany(u => u.Posts)
            .WithOne(p => p.Author)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Entity<User>()
            .HasMany(u => u.PostLikes)
            .WithOne(p => p.User)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Entity<Post>()
            .HasMany(u => u.Likes)
            .WithOne(p => p.Post)
            .OnDelete(DeleteBehavior.Cascade);
    }
}