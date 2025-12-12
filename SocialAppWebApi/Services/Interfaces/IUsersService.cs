using SocialAppWebApi.Data;

namespace SocialAppWebApi.Services.Interfaces;

/// <summary>
/// Provides lookup access to the registered users
/// </summary>
public interface IUsersService
{
    /// <summary>
    /// Retrieve a user by ID
    /// </summary>
    /// <param name="id">The ID of the user</param>
    /// <returns>A User object if the ID is valid and exists in the database, or null otherwise</returns>
    public Task<User?> GetUserByIdAsync(long id);
    
    /// <summary>
    /// Retrieve a user by username
    /// </summary>
    /// <param name="username">The username of the user</param>
    /// <returns>A User object if the username is valid and exists in the database, or null otherwise</returns>
    public Task<User?> GetUserByNameAsync(string username);
}