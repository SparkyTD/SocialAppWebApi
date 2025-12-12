using SocialAppWebApi.Data;

namespace SocialAppWebApi.Services.Interfaces;

public interface IPostsService
{
    public IEnumerable<Post> GetPosts(int page, int pageSize);
    public Task<Post?> GetPostByIdAsync(long id);
    public Task<Post> SavePostAsync(Post post);
    public Task<bool> DeletePostByIdAsync(long postId, long deletingUserId);
}