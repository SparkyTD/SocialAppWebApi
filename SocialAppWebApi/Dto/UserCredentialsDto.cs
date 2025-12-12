using System.ComponentModel.DataAnnotations;

namespace SocialAppWebApi.Dto;

public class UserCredentialsDto
{
    [Required, StringLength(32, MinimumLength = 1)] public required string Username { get; set; }
    [Required, StringLength(128, MinimumLength = 4)] public required string Password { get; set; }
}