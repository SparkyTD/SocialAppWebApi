using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SocialAppWebApi.Data;
using SocialAppWebApi.Dto;
using SocialAppWebApi.Endpoints;
using SocialAppWebApi.Services.Interfaces;

namespace SocialAppWebApi.Test.Endpoints;

public class PostsControllerTests
{
    private Mock<IPostsService> _postsServiceMock = null!;
    private Mock<IUsersService> _usersServiceMock = null!;
    private Mock<IMapper> _mapperMock = null!;
    private PostsController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _postsServiceMock = new Mock<IPostsService>();
        _usersServiceMock = new Mock<IUsersService>();
        _mapperMock = new Mock<IMapper>();
        _controller = new PostsController(_postsServiceMock.Object, _usersServiceMock.Object, _mapperMock.Object);
    }
    
    #region GetPosts

    [Test]
    public void GetPosts_WithValidParams_ReturnsOk()
    {
        // Arrange
        _postsServiceMock
            .Setup(s => s.GetPosts(1, 10))
            .Returns([]);
        _mapperMock
            .Setup(m => m.Map<IEnumerable<PostDto>>(It.IsAny<List<Post>>()))
            .Returns([]);

        // Act
        var result = _controller.GetPosts(page: 1, pageSize: 10);

        // Assert
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public void GetPosts_WithValidParams_CallsServiceWithCorrectParams()
    {
        // Arrange
        _postsServiceMock
            .Setup(s => s.GetPosts(3, 25))
            .Returns([]);

        // Act
        _controller.GetPosts(page: 3, pageSize: 25);

        // Assert
        _postsServiceMock.Verify(s => s.GetPosts(3, 25), Times.Once);
    }

    [Test]
    public void GetPosts_WithPosts_ReturnsMappedDtos()
    {
        // Arrange
        var posts = new List<Post> { CreateTestPost(), CreateTestPost() };
        var dtos = new List<PostDto> { new(), new() };
        _postsServiceMock
            .Setup(s => s.GetPosts(1, 10))
            .Returns(posts);
        _mapperMock
            .Setup(m => m.Map<IEnumerable<PostDto>>(It.IsAny<List<Post>>()))
            .Returns(dtos);

        // Act
        var result = _controller.GetPosts(page: 1, pageSize: 10).Result as OkObjectResult;

        // Assert
        Assert.That(result!.Value, Is.EqualTo(dtos));
    }

    [TestCase(0, 10)]
    [TestCase(-1, 10)]
    [TestCase(1, 0)]
    [TestCase(1, -1)]
    [TestCase(1, 101)]
    public void GetPosts_WithInvalidParams_ReturnsBadRequest(int page, int pageSize)
    {
        // Arrange
        // (no setup needed)

        // Act
        var result = _controller.GetPosts(page: page, pageSize: pageSize);

        // Assert
        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public void GetPosts_WithInvalidParams_DoesNotCallService()
    {
        // Arrange
        // (no setup needed)

        // Act
        _controller.GetPosts(page: 0, pageSize: 10);

        // Assert
        _postsServiceMock.Verify(s => s.GetPosts(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    #endregion

    #region GetPost

    [Test]
    public async Task GetPost_WithExistingId_ReturnsOk()
    {
        // Arrange
        var post = CreateTestPost();
        _postsServiceMock
            .Setup(s => s.GetPostByIdAsync(1))
            .ReturnsAsync(post);
        _mapperMock
            .Setup(m => m.Map<PostDto>(post))
            .Returns(new PostDto());

        // Act
        var result = await _controller.GetPost(1);

        // Assert
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task GetPost_WithExistingId_ReturnsMappedDto()
    {
        // Arrange
        var post = CreateTestPost();
        var dto = new PostDto { Content = "Test content" };
        _postsServiceMock
            .Setup(s => s.GetPostByIdAsync(1))
            .ReturnsAsync(post);
        _mapperMock
            .Setup(m => m.Map<PostDto>(post))
            .Returns(dto);

        // Act
        var result = await _controller.GetPost(1);
        var okResult = result.Result as OkObjectResult;

        // Assert
        Assert.That(okResult!.Value, Is.EqualTo(dto));
    }

    [Test]
    public async Task GetPost_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        _postsServiceMock
            .Setup(s => s.GetPostByIdAsync(999))
            .ReturnsAsync((Post?)null);

        // Act
        var result = await _controller.GetPost(999);

        // Assert
        Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
    }

    #endregion

    #region CreatePost

    [Test]
    public async Task CreatePost_WithValidUser_ReturnsOk()
    {
        // Arrange
        var user = Createadmin();
        SetupAuthenticatedUser(user);
        var createDto = new CreatePostDto { Body = "New post content" };
        _postsServiceMock
            .Setup(s => s.SavePostAsync(It.IsAny<Post>()))
            .ReturnsAsync(CreateTestPost());
        _mapperMock
            .Setup(m => m.Map<PostDto>(It.IsAny<Post>()))
            .Returns(new PostDto());

        // Act
        var result = await _controller.CreatePost(createDto);

        // Assert
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task CreatePost_WithValidUser_SavesPostWithCorrectData()
    {
        // Arrange
        var user = Createadmin();
        SetupAuthenticatedUser(user);
        var createDto = new CreatePostDto { Body = "New post content" };
        Post? savedPost = null;
        _postsServiceMock
            .Setup(s => s.SavePostAsync(It.IsAny<Post>()))
            .Callback<Post>(p => savedPost = p)
            .ReturnsAsync(CreateTestPost());

        // Act
        await _controller.CreatePost(createDto);

        // Assert
        Assert.That(savedPost, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(savedPost!.Content, Is.EqualTo("New post content"));
            Assert.That(savedPost.Author, Is.EqualTo(user));
            Assert.That(savedPost.CachedLikeCount, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task CreatePost_WithValidUser_ReturnsMappedDto()
    {
        // Arrange
        var user = Createadmin();
        SetupAuthenticatedUser(user);
        var createDto = new CreatePostDto { Body = "Content" };
        var savedPost = CreateTestPost();
        var dto = new PostDto { Content = "Content" };
        _postsServiceMock
            .Setup(s => s.SavePostAsync(It.IsAny<Post>()))
            .ReturnsAsync(savedPost);
        _mapperMock
            .Setup(m => m.Map<PostDto>(savedPost))
            .Returns(dto);

        // Act
        var result = await _controller.CreatePost(createDto);
        var okResult = result.Result as OkObjectResult;

        // Assert
        Assert.That(okResult!.Value, Is.EqualTo(dto));
    }

    [Test]
    public async Task CreatePost_WhenUserNotFound_ReturnsUnauthorized()
    {
        // Arrange
        SetupUnauthenticatedUser();
        var createDto = new CreatePostDto { Body = "Content" };

        // Act
        var result = await _controller.CreatePost(createDto);

        // Assert
        Assert.That(result.Result, Is.TypeOf<UnauthorizedResult>());
    }

    [Test]
    public async Task CreatePost_WhenUserNotFound_DoesNotCallService()
    {
        // Arrange
        SetupUnauthenticatedUser();
        var createDto = new CreatePostDto { Body = "Content" };

        // Act
        await _controller.CreatePost(createDto);

        // Assert
        _postsServiceMock.Verify(s => s.SavePostAsync(It.IsAny<Post>()), Times.Never);
    }

    #endregion

    #region DeletePost

    [Test]
    public async Task DeletePost_AsAuthor_ReturnsOk()
    {
        // Arrange
        var user = Createadmin();
        SetupAuthenticatedUser(user);
        _postsServiceMock
            .Setup(s => s.DeletePostByIdAsync(1, user.Id))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeletePost(1);

        // Assert
        Assert.That(result, Is.TypeOf<OkResult>());
    }

    [Test]
    public async Task DeletePost_AsAuthor_CallsServiceWithCorrectParams()
    {
        // Arrange
        var user = Createadmin();
        SetupAuthenticatedUser(user);
        _postsServiceMock
            .Setup(s => s.DeletePostByIdAsync(42, user.Id))
            .ReturnsAsync(true);

        // Act
        await _controller.DeletePost(42);

        // Assert
        _postsServiceMock.Verify(s => s.DeletePostByIdAsync(42, user.Id), Times.Once);
    }

    [Test]
    public async Task DeletePost_WhenPostNotFoundOrNotOwned_ReturnsNotFound()
    {
        // Arrange
        var user = Createadmin();
        SetupAuthenticatedUser(user);
        _postsServiceMock
            .Setup(s => s.DeletePostByIdAsync(1, user.Id))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeletePost(1);

        // Assert
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task DeletePost_WhenPostNotFoundOrNotOwned_ReturnsErrorMessage()
    {
        // Arrange
        var user = Createadmin();
        SetupAuthenticatedUser(user);
        _postsServiceMock
            .Setup(s => s.DeletePostByIdAsync(1, user.Id))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeletePost(1) as NotFoundObjectResult;

        // Assert
        Assert.That(result!.Value, Is.EqualTo("The specified post cannot be found."));
    }

    [Test]
    public async Task DeletePost_WhenUserNotFound_ReturnsUnauthorized()
    {
        // Arrange
        SetupUnauthenticatedUser();

        // Act
        var result = await _controller.DeletePost(1);

        // Assert
        Assert.That(result, Is.TypeOf<UnauthorizedResult>());
    }

    [Test]
    public async Task DeletePost_WhenUserNotFound_DoesNotCallService()
    {
        // Arrange
        SetupUnauthenticatedUser();

        // Act
        await _controller.DeletePost(1);

        // Assert
        _postsServiceMock.Verify(s => s.DeletePostByIdAsync(It.IsAny<long>(), It.IsAny<long>()), Times.Never);
    }

    #endregion
    
    private static User Createadmin() => new()
    {
        Id = 1,
        Username = "admin",
        PasswordHash = "hash",
        Posts = [],
        PostLikes = []
    };

    private static Post CreateTestPost() => new()
    {
        Id = 1,
        Content = "Test content",
        CreatedAt = DateTime.UtcNow,
        AuthorId = 1,
        Author = Createadmin(),
        CachedLikeCount = 0,
        Likes = []
    };

    private void SetupAuthenticatedUser(User user)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        _usersServiceMock
            .Setup(s => s.GetUserByIdAsync(user.Id))
            .ReturnsAsync(user);
    }

    private void SetupUnauthenticatedUser()
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "999") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        _usersServiceMock
            .Setup(s => s.GetUserByIdAsync(999))
            .ReturnsAsync((User?)null);
    }

}