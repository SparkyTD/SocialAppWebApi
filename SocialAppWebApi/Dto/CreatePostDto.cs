using System.ComponentModel.DataAnnotations;

namespace SocialAppWebApi.Dto;

public class CreatePostDto
{
    [StringLength(240, MinimumLength = 1)]
    public required string Body { get; set; }
}