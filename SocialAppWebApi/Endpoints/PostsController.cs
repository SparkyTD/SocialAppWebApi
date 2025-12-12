using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialAppWebApi.Data;
using SocialAppWebApi.Dto;
using SocialAppWebApi.Services;

namespace SocialAppWebApi.Endpoints;

[Authorize]
[ApiController]
[Route("v1/[controller]")]
public class PostsController(PostsService postsService, UsersService usersService, IMapper mapper) : UserControllerBase(usersService)
{
    [HttpGet]
    public ActionResult<IEnumerable<Post>> GetPosts(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest("Invalid pagination parameters");
        }

        var posts = postsService
            .GetPosts(page, pageSize)
            .ToList();

        return Ok(mapper.Map<IEnumerable<PostDto>>(posts));
    }
    
    [HttpGet("{id:long}")]
    public async Task<ActionResult<Post>> GetPost(long id)
    {
        var post = await postsService.GetPostByIdAsync(id);

        if (post == null)
            return NotFound();

        return Ok(mapper.Map<PostDto>(post));
    }

    [HttpPost]
    public async Task<ActionResult<PostDto>> CreatePost([FromBody] CreatePostDto createPostDto)
    {
        if (await GetCurrentUserAsync() is not {} user)
            return Unauthorized();
        
        var post = new Post
        {
            Author = user,
            Content = createPostDto.Body,
            CreatedAt = DateTime.UtcNow,
            CachedLikeCount = 0,
            Likes = []
        };
        
        var entity = await postsService.SavePostAsync(post);
        
        return Ok(mapper.Map<PostDto>(entity));
    }

    [HttpDelete("{id:long}")]
    public async Task<ActionResult> DeletePost(long id)
    {
        if (await GetCurrentUserAsync() is not {} user)
            return Unauthorized();
        
        
        
        if (!await postsService.DeletePostByIdAsync(id, user.Id))
            return NotFound("The specified post cannot be found.");
        
        return Ok();
    }
}