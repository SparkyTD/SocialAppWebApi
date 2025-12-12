namespace SocialAppWebApi.Dto;

public class PostDto
{
    public long Id { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public long LikeCount { get; set; }
    public string Author { get; set; }
}