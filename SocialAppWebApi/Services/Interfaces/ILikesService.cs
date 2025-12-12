using SocialAppWebApi.Data;

namespace SocialAppWebApi.Services.Interfaces;

public interface ILikesService
{
    public Task<bool> CreateLikeAsync(User user, long postId);
    public Task<bool> DeleteLikeAsync(User user, long postId);
    public Task UpdateAllCachedLikeCountsAsync();
}