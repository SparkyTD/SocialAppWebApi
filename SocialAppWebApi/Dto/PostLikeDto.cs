using System.ComponentModel.DataAnnotations;

namespace SocialAppWebApi.Dto;

public class PostLikeDto
{
    [Required] public required long PostId { get; set; }
}