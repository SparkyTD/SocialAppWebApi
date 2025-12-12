using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SocialAppWebApi.Data;
using SocialAppWebApi.Services;

namespace SocialAppWebApi.Endpoints;

public class UserControllerBase(UsersService usersService) : ControllerBase
{
    protected async Task<User?> GetCurrentUserAsync()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !long.TryParse(userIdClaim, out long userId))
            return null;

        return await usersService.GetUserByIdAsync(userId);
    }
}