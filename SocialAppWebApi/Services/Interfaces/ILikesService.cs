using SocialAppWebApi.Data;

namespace SocialAppWebApi.Services.Interfaces;

/// <summary>
/// Manages post like operations like creating, deleting and reconciling cached like counts.
/// </summary>
public interface ILikesService
{
    /// <summary>
    /// Creates a like for the specified post, by the given user
    /// </summary>
    /// <param name="user">The user liking the post</param>
    /// <param name="postId">The ID of the post being liked</param>
    /// <returns>True if the like was created and saved, false if it already existed</returns>
    public Task<bool> CreateLikeAsync(User user, long postId);
    
    /// <summary>
    /// Deletes a like record for the specified post, by the given user
    /// </summary>
    /// <param name="user">The user removing the like from the post</param>
    /// <param name="postId">The ID of the post being un-liked</param>
    /// <returns>True if the like was successfully removed, false if no like existed, or the specified user didn't "own" the like</returns>
    public Task<bool> DeleteLikeAsync(User user, long postId);
    
    /// <summary>
    /// Forcefully updates the cached like values of all posts, for reconciliation purposes
    /// </summary>
    /// <returns></returns>
    public Task UpdateAllCachedLikeCountsAsync();
}