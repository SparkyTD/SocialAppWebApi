namespace SocialAppWebApi.Dto;

public class PostDto
{
    public long Id { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public long LikeCount { get; set; }
    public required string Author { get; set; }
}