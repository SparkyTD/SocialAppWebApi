using SocialAppWebApi.Data;

namespace SocialAppWebApi.Services.Interfaces;

/// <summary>
/// Manages user authentication operations
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Attempts to register a new user with the given username and password
    /// </summary>
    /// <param name="username">The username of the user to be created</param>
    /// <param name="password">The password of the user to be created</param>
    /// <returns>The newly created User record in case of success, null otherwise</returns>
    public Task<User?> RegisterAsync(string username, string password);
    
    /// <summary>
    /// Verifies a user login given the user's credentials
    /// </summary>
    /// <param name="username">The username of the user to be authenticated</param>
    /// <param name="password">The password of the user to be authenticated</param>
    /// <returns>The corresponding User object in case of successful login, null otherwise</returns>
    public Task<User?> LoginAsync(string username, string password);
    
    /// <summary>
    /// Generates a new JWT authentication token for the specified user
    /// </summary>
    /// <param name="user">The authenticated user that will use the token</param>
    /// <returns>A JWT token string</returns>
    public string GenerateJwtToken(User user);
}