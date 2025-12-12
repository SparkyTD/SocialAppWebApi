using System.ComponentModel.DataAnnotations;

namespace SocialAppWebApi.Data;

public class Post
{
    [Key] public long Id { get; set; }
    [StringLength(240, MinimumLength = 1)] public required string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public long CachedLikeCount { get; set; }
    public long AuthorId { get; set; }
    public required User Author { get; set; }
    public required List<PostLike> Likes { get; set; }
}