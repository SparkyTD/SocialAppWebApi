using SocialAppWebApi.Data;

namespace SocialAppWebApi.Services.Interfaces;

public interface IUsersService
{
    public Task<User?> GetUserByIdAsync(long id);
    public Task<User?> GetUserByNameAsync(string username);
}