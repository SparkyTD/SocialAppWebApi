using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SocialAppWebApi.Data;
using SocialAppWebApi.Dto;
using SocialAppWebApi.Endpoints;
using SocialAppWebApi.Services.Interfaces;

namespace SocialAppWebApi.Test.Endpoints;

public class LikesControllerTests
{
    private Mock<IUsersService> usersServiceMock = null!;
    private Mock<ILikesService> likesServiceMock = null!;
    private LikesController controller = null!;
    
    [SetUp]
    public void SetUp()
    {
        usersServiceMock = new Mock<IUsersService>();
        likesServiceMock = new Mock<ILikesService>();
        controller = new LikesController(usersServiceMock.Object, likesServiceMock.Object);
    }
    
    #region CreateLike

    [Test]
    public async Task CreateLike_WithValidUser_ReturnsOk()
    {
        // Arrange
        var user = CreateTestUser();
        var likeDto = new PostLikeDto { PostId = 1 };
        SetupAuthenticatedUser(user);
        likesServiceMock
            .Setup(s => s.CreateLikeAsync(user, 1))
            .ReturnsAsync(true);

        // Act
        var result = await controller.CreateLike(likeDto);

        // Assert
        Assert.That(result, Is.TypeOf<OkResult>());
    }

    [Test]
    public async Task CreateLike_WithValidUser_CallsService()
    {
        // Arrange
        var user = CreateTestUser();
        var likeDto = new PostLikeDto { PostId = 42 };
        SetupAuthenticatedUser(user);
        likesServiceMock
            .Setup(s => s.CreateLikeAsync(user, 42))
            .ReturnsAsync(true);

        // Act
        await controller.CreateLike(likeDto);

        // Assert
        likesServiceMock.Verify(s => s.CreateLikeAsync(user, 42), Times.Once);
    }

    [Test]
    public async Task CreateLike_WhenAlreadyLiked_ReturnsBadRequest()
    {
        // Arrange
        var user = CreateTestUser();
        var likeDto = new PostLikeDto { PostId = 1 };
        SetupAuthenticatedUser(user);
        likesServiceMock
            .Setup(s => s.CreateLikeAsync(user, 1))
            .ReturnsAsync(false);

        // Act
        var result = await controller.CreateLike(likeDto);

        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task CreateLike_WhenAlreadyLiked_ReturnsErrorMessage()
    {
        // Arrange
        var user = CreateTestUser();
        var likeDto = new PostLikeDto { PostId = 1 };
        SetupAuthenticatedUser(user);
        likesServiceMock
            .Setup(s => s.CreateLikeAsync(user, 1))
            .ReturnsAsync(false);

        // Act
        var result = await controller.CreateLike(likeDto) as BadRequestObjectResult;

        // Assert
        Assert.That(result!.Value, Is.EqualTo("The current user has already liked this post"));
    }

    [Test]
    public async Task CreateLike_WhenUserNotFound_ReturnsUnauthorized()
    {
        // Arrange
        var likeDto = new PostLikeDto { PostId = 1 };
        SetupUnauthenticatedUser();

        // Act
        var result = await controller.CreateLike(likeDto);

        // Assert
        Assert.That(result, Is.TypeOf<UnauthorizedResult>());
    }

    [Test]
    public async Task CreateLike_WhenUserNotFound_DoesNotCallService()
    {
        // Arrange
        var likeDto = new PostLikeDto { PostId = 1 };
        SetupUnauthenticatedUser();

        // Act
        await controller.CreateLike(likeDto);

        // Assert
        likesServiceMock.Verify(s => s.CreateLikeAsync(It.IsAny<User>(), It.IsAny<long>()), Times.Never);
    }

    #endregion

    #region DeleteLike

    [Test]
    public async Task DeleteLike_WithValidUser_ReturnsOk()
    {
        // Arrange
        var user = CreateTestUser();
        var likeDto = new PostLikeDto { PostId = 1 };
        SetupAuthenticatedUser(user);
        likesServiceMock
            .Setup(s => s.DeleteLikeAsync(user, 1))
            .ReturnsAsync(true);

        // Act
        var result = await controller.DeleteLike(likeDto);

        // Assert
        Assert.That(result, Is.TypeOf<OkResult>());
    }

    [Test]
    public async Task DeleteLike_WithValidUser_CallsService()
    {
        // Arrange
        var user = CreateTestUser();
        var likeDto = new PostLikeDto { PostId = 42 };
        SetupAuthenticatedUser(user);
        likesServiceMock
            .Setup(s => s.DeleteLikeAsync(user, 42))
            .ReturnsAsync(true);

        // Act
        await controller.DeleteLike(likeDto);

        // Assert
        likesServiceMock.Verify(s => s.DeleteLikeAsync(user, 42), Times.Once);
    }

    [Test]
    public async Task DeleteLike_WhenNotLiked_ReturnsBadRequest()
    {
        // Arrange
        var user = CreateTestUser();
        var likeDto = new PostLikeDto { PostId = 1 };
        SetupAuthenticatedUser(user);
        likesServiceMock
            .Setup(s => s.DeleteLikeAsync(user, 1))
            .ReturnsAsync(false);

        // Act
        var result = await controller.DeleteLike(likeDto);

        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task DeleteLike_WhenNotLiked_ReturnsErrorMessage()
    {
        // Arrange
        var user = CreateTestUser();
        var likeDto = new PostLikeDto { PostId = 1 };
        SetupAuthenticatedUser(user);
        likesServiceMock
            .Setup(s => s.DeleteLikeAsync(user, 1))
            .ReturnsAsync(false);

        // Act
        var result = await controller.DeleteLike(likeDto) as BadRequestObjectResult;

        // Assert
        Assert.That(result!.Value, Is.EqualTo("The current user has not liked this post yet"));
    }

    [Test]
    public async Task DeleteLike_WhenUserNotFound_ReturnsUnauthorized()
    {
        // Arrange
        var likeDto = new PostLikeDto { PostId = 1 };
        SetupUnauthenticatedUser();

        // Act
        var result = await controller.DeleteLike(likeDto);

        // Assert
        Assert.That(result, Is.TypeOf<UnauthorizedResult>());
    }

    [Test]
    public async Task DeleteLike_WhenUserNotFound_DoesNotCallService()
    {
        // Arrange
        var likeDto = new PostLikeDto { PostId = 1 };
        SetupUnauthenticatedUser();

        // Act
        await controller.DeleteLike(likeDto);

        // Assert
        likesServiceMock.Verify(s => s.DeleteLikeAsync(It.IsAny<User>(), It.IsAny<long>()), Times.Never);
    }

    #endregion
    
    private static User CreateTestUser() => new()
    {
        Id = 1,
        Username = "testuser",
        PasswordHash = "hash",
        Posts = [],
        PostLikes = []
    };

    private void SetupAuthenticatedUser(User user)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        usersServiceMock
            .Setup(s => s.GetUserByIdAsync(user.Id))
            .ReturnsAsync(user);
    }

    private void SetupUnauthenticatedUser()
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "999") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        usersServiceMock
            .Setup(s => s.GetUserByIdAsync(999))
            .ReturnsAsync((User?)null);
    }
}