using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SocialAppWebApi.Data;
using SocialAppWebApi.Services.Interfaces;

namespace SocialAppWebApi.Services;

public class AuthService(AppDatabase database, IUsersService usersService, TokenValidationParameters jwtTokenParameters) : IAuthService
{
    public async Task<User?> RegisterAsync(string username, string password)
    {
        if (await database.Users.AnyAsync(u => u.Username == username))
            return null;

        var user = new User
        {
            Id = 0,
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Posts = [],
            PostLikes = []
        };

        try
        {
            await database.Users.AddAsync(user);
            await database.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Another request has already registered the same username
            return null;
        }
        
        return user;
    }

    public async Task<User?> LoginAsync(string username, string password)
    {
        var user = await usersService.GetUserByNameAsync(username);
        if (user == null)
            return null;

        return !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash) ? null : user;
    }

    public string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
        };

        var token = new JwtSecurityToken(
            issuer: jwtTokenParameters.ValidIssuer,
            audience: jwtTokenParameters.ValidAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: new SigningCredentials(jwtTokenParameters.IssuerSigningKey, SecurityAlgorithms.HmacSha256)
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}