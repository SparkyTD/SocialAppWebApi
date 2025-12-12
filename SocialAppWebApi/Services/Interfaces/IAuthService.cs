using SocialAppWebApi.Data;

namespace SocialAppWebApi.Services.Interfaces;

public interface IAuthService
{
    public Task<User?> RegisterAsync(string username, string password);
    public Task<User?> LoginAsync(string username, string password);
    public string GenerateJwtToken(User user);
}