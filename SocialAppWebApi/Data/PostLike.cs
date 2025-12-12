using Microsoft.EntityFrameworkCore;

namespace SocialAppWebApi.Data;

[PrimaryKey(nameof(PostId), nameof(UserId))]
public class PostLike
{
    public long PostId { get; set; }
    public Post Post { get; set; }
    public long UserId { get; set; }
    public User User { get; set; }
    public required DateTime CreatedAt { get; set; }
}