using Microsoft.EntityFrameworkCore;
using SocialAppWebApi.Data;

namespace SocialAppWebApi.Services;

public class LikesService(AppDatabase database)
{
    private static volatile bool shouldUpdateLikeCountCaches = false;
    
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

            shouldUpdateLikeCountCaches = true;
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
        database.PostLikes.RemoveRange(database.PostLikes
            .Where(p => p.PostId == postId)
            .Where(p => p.UserId == user.Id));
        
        var updatedEntryCount = await database.SaveChangesAsync();
        
        if (updatedEntryCount != 0)
            shouldUpdateLikeCountCaches = true;
        
        return updatedEntryCount != 0;
    }

    public async Task UpdateAllCachedLikeCountsAsync()
    {
        if (!shouldUpdateLikeCountCaches)
            return;
        
        await database.Posts.ExecuteUpdateAsync(setters => setters
            .SetProperty(p => p.CachedLikeCount, p => database.PostLikes.Count(pl => pl.PostId == p.Id)));
        
        shouldUpdateLikeCountCaches = false;
    }
}