using Microsoft.EntityFrameworkCore;
using SocialAppWebApi.Data;

namespace SocialAppWebApi.Services;

public class UsersService(AppDatabase database)
{
    public async Task<User?> GetUserByIdAsync(long id)
    {
        var user = await database.Users.FindAsync(id);
        return user;
    }
    
    public async Task<User?> GetUserByNameAsync(string username)
    {
        var user = await database.Users.FirstOrDefaultAsync(u => u.Username == username);
        return user;
    }
}