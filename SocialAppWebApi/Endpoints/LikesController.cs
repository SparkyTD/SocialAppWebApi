using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialAppWebApi.Dto;
using SocialAppWebApi.Services.Interfaces;

namespace SocialAppWebApi.Endpoints;

[Authorize]
[ApiController]
[Route("v1/[controller]")]
public class LikesController(IUsersService usersService, ILikesService likesService, IPostsService postsService) : UserControllerBase(usersService)
{
    [HttpPut]
    public async Task<IActionResult> CreateLike([FromBody] PostLikeDto likeDto)
    {
        if (await GetCurrentUserAsync() is not {} user)
            return Unauthorized();

        if (await postsService.GetPostByIdAsync(likeDto.PostId) == null)
            return NotFound();
        
        if (!await likesService.CreateLikeAsync(user, likeDto.PostId))
            return BadRequest("The current user has already liked this post");
        
        return Ok();
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteLike([FromBody] PostLikeDto likeDto)
    {
        if (await GetCurrentUserAsync() is not {} user)
            return Unauthorized();
        
        if (await postsService.GetPostByIdAsync(likeDto.PostId) == null)
            return NotFound();
        
        if (!await likesService.DeleteLikeAsync(user, likeDto.PostId))
            return BadRequest("The current user has not liked this post yet");
        
        return Ok();
    }
}