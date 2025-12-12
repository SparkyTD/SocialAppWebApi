using System.ComponentModel.DataAnnotations;

namespace SocialAppWebApi.Data;

public class User
{
    [Key] public long Id { get; set; }
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public required List<Post> Posts { get; set; }
    public required List<PostLike> PostLikes { get; set; }
}