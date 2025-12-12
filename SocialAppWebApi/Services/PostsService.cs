using Microsoft.EntityFrameworkCore;
using SocialAppWebApi.Data;

namespace SocialAppWebApi.Services;

public class PostsService(AppDatabase database)
{
    public IQueryable<Post> GetPosts(int page, int pageSize)
    {
        return database.Posts
            .Include(p => p.Author)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
    }

    public Post? GetPostById(long id)
    {
        return database.Posts.FirstOrDefault(p => p.Id == id);
    }
    
    public async Task<Post> SavePostAsync(Post post)
    {
        var entry = database.Posts.Add(post);
        await database.SaveChangesAsync();
        return entry.Entity;
    }

    public async Task<bool> DeletePostByIdAsync(long id)
    {
        var post = await database.Posts.FirstOrDefaultAsync(p => p.Id == id);
        if (post == null)
            return false;
        
        database.Posts.Remove(post);
        await database.SaveChangesAsync();
        return true;
    }
}