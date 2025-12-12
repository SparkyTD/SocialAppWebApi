using SocialAppWebApi.Data;

namespace SocialAppWebApi.Services.Interfaces;

/// <summary>
/// Manages post operations including creating, deleting and retrieving posts
/// </summary>
public interface IPostsService
{
    /// <summary>
    /// Get a paginated list of all posts
    /// </summary>
    /// <param name="page">The current page being queried. Must be at least 1</param>
    /// <param name="pageSize">The number of items on a page. Must be at least 1</param>
    /// <returns>A in-memory list of matching Post objects</returns>
    public IEnumerable<Post> GetPosts(int page, int pageSize);
    
    /// <summary>
    /// Retrieves a post by ID
    /// </summary>
    /// <param name="id">The ID of the post</param>
    /// <returns>A matching Post object if the ID is valid, or null otherwise</returns>
    public Task<Post?> GetPostByIdAsync(long id);
    
    /// <summary>
    /// Saves a new post in the database
    /// </summary>
    /// <param name="post">The Post object to be saved</param>
    /// <returns>The tracked entity of the inserted Post object</returns>
    public Task<Post> SavePostAsync(Post post);
    
    /// <summary>
    /// Attempts to delete a Post created by the specified user
    /// </summary>
    /// <param name="postId">The ID of the post to be deleted</param>
    /// <param name="deletingUserId">The ID of the user attempting to delete the post</param>
    /// <returns>True if the post was successfully deleted, false if the post didn't exist or the specified user isn't the owner of the post</returns>
    public Task<bool> DeletePostByIdAsync(long postId, long deletingUserId);
}