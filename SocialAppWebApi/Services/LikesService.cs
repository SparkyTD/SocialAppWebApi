using Microsoft.EntityFrameworkCore;
using SocialAppWebApi.Data;
using SocialAppWebApi.Services.Interfaces;

namespace SocialAppWebApi.Services;

public class LikesService(AppDatabase database) : ILikesService
{
    public async Task<bool> CreateLikeAsync(User user, long postId)
    {
        // If there's already a locally tracked entity with matching data,
        // skip this operation entirely and return false.
        var trackedEntity = database.PostLikes.Local
            .FirstOrDefault(pl => pl.PostId == postId && pl.UserId == user.Id);
    
        if (trackedEntity != null)
            return false;

        await using var transaction = await database.Database.BeginTransactionAsync();

        try
        {
            var postLike = new PostLike
            {
                PostId = postId,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
            };
            
            database.PostLikes.Add(postLike);
            await database.SaveChangesAsync();
            
            // The atomicity of this operation is guaranteed by the database engine itself.
            // This prevents data inconsistencies and race conditions that would result from
            // a simpler SELECT / UPDATE pattern.
            await database.Posts
                .Where(p => p.Id == postId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.CachedLikeCount, p => p.CachedLikeCount + 1));

            await transaction.CommitAsync();
            
            return true;
        }
        catch (DbUpdateException)
        {
            // Thrown when we try to insert a duplicate record (PKs: {PostId, UserId})
            await transaction.RollbackAsync();
            return false;
        }
    }

    public async Task<bool> DeleteLikeAsync(User user, long postId)
    {
        await using var transaction = await database.Database.BeginTransactionAsync();
        
        try
        {
            var deletedCount = await database.PostLikes
                .Where(p => p.PostId == postId && p.UserId == user.Id)
                .ExecuteDeleteAsync();

            if (deletedCount > 0)
            {
                await database.Posts
                    .Where(p => p.Id == postId)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.CachedLikeCount, p => Math.Max(0, p.CachedLikeCount - 1)));
            }

            await transaction.CommitAsync();
            
            return deletedCount != 0;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateAllCachedLikeCountsAsync()
    {
        // Cache reconciliation for consistency
        await database.Posts.ExecuteUpdateAsync(setters => setters
            .SetProperty(p => p.CachedLikeCount, p => database.PostLikes.Count(pl => pl.PostId == p.Id)));
    }
}