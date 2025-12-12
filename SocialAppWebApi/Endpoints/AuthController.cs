using Microsoft.AspNetCore.Mvc;
using SocialAppWebApi.Dto;
using SocialAppWebApi.Services;

namespace SocialAppWebApi.Endpoints;

[ApiController]
[Route("[controller]")]
public class AuthController(AuthService authService) : ControllerBase
{
    [HttpPost]
    [Route(nameof(Register))]
    public async Task<IActionResult> Register([FromBody] UserCredentialsDto credentialsDto)
    {
        var user = await authService.RegisterAsync(credentialsDto.Username, credentialsDto.Password);
        if (user == null)
            return BadRequest("Invalid username or password");
        
        return Ok();
    }

    [HttpPost]
    [Route(nameof(Login))]
    public async Task<IActionResult> Login([FromBody] UserCredentialsDto credentialsDto)
    {
        var user = await authService.LoginAsync(credentialsDto.Username, credentialsDto.Password);
        if (user == null)
            return BadRequest("Invalid username or password");

        var token = authService.GenerateJwtToken(user);
        
        return Ok(token);
    }
}