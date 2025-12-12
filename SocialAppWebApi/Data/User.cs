using System.ComponentModel.DataAnnotations;

namespace SocialAppWebApi.Data;

public class User
{
    [Key] public long Id { get; set; }
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public List<Post> Posts { get; set; } = [];
    public List<PostLike> PostLikes { get; set; } = [];
}