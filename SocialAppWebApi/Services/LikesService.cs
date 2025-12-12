using Microsoft.EntityFrameworkCore;
using SocialAppWebApi.Data;

namespace SocialAppWebApi.Services;

public class LikesService(AppDatabase database)
{
    public async Task<bool> CreateLikeAsync(User user, long postId)
    {
        var postLike = new PostLike
        {
            PostId = postId,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
        };

        try
        {
            database.PostLikes.Add(postLike);
            await database.SaveChangesAsync();
            
            // The atomicity of this operation is guaranteed by the database engine itself.
            // This prevents data inconsistencies and race conditions that would result from
            // a simpler SELECT / UPDATE pattern.
            await database.Posts
                .Where(p => p.Id == postId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.CachedLikeCount, p => p.CachedLikeCount + 1));
        }
        catch (DbUpdateException)
        {
            // Thrown when we try to insert a duplicate record (PKs: {PostId, UserId})
            return false;
        }
        
        return true;
    }

    public async Task<bool> DeleteLikeAsync(User user, long postId)
    {
        var deletedCount = await database.PostLikes
            .Where(p => p.PostId == postId && p.UserId == user.Id)
            .ExecuteDeleteAsync();
        
        await database.Posts
            .Where(p => p.Id == postId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.CachedLikeCount, p => Math.Max(0, p.CachedLikeCount - 1)));
        
        return deletedCount != 0;
    }

    public async Task UpdateAllCachedLikeCountsAsync()
    {
        // Cache reconciliation for consistency
        await database.Posts.ExecuteUpdateAsync(setters => setters
            .SetProperty(p => p.CachedLikeCount, p => database.PostLikes.Count(pl => pl.PostId == p.Id)));
    }
}