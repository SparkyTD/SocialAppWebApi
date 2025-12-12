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
            Created = DateTime.UtcNow,
        };

        try
        {
            database.PostLikes.Add(postLike);
            await database.SaveChangesAsync();
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
        
        return await database.SaveChangesAsync() != 0;
    }

    public async Task UpdateAllCachedLikeCountsAsync()
    {
        
    }
}