using Microsoft.EntityFrameworkCore;
using SocialAppWebApi.Data;
using SocialAppWebApi.Services.Interfaces;

namespace SocialAppWebApi.Services;

public class PostsService(AppDatabase database) : IPostsService
{
    public IEnumerable<Post> GetPosts(int page, int pageSize)
    {
        return database.Posts
            .Include(p => p.Author)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsEnumerable(); // Enumerate into memory here
    }

    public async Task<Post?> GetPostByIdAsync(long id)
    {
        return await database.Posts.FirstOrDefaultAsync(p => p.Id == id);
    }
    
    public async Task<Post> SavePostAsync(Post post)
    {
        var entry = database.Posts.Add(post);
        await database.SaveChangesAsync();
        return entry.Entity;
    }

    public async Task<bool> DeletePostByIdAsync(long postId, long deletingUserId)
    {
        // Users should only be able to delete their own posts
        var deleted = await database.Posts
            .Where(p => p.Id == postId)
            .Where(p => p.AuthorId == deletingUserId)
            .ExecuteDeleteAsync();
        return deleted > 0;
    }
}