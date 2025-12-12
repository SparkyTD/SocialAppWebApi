using Microsoft.AspNetCore.Mvc;
using Moq;
using SocialAppWebApi.Data;
using SocialAppWebApi.Dto;
using SocialAppWebApi.Endpoints;
using SocialAppWebApi.Services.Interfaces;

namespace SocialAppWebApi.Test.Endpoints;

public class AuthControllerTests
{
    private Mock<IAuthService> authServiceMock = null!;
    private AuthController controller = null!;
    
    [SetUp]
    public void Setup()
    {
        authServiceMock = new Mock<IAuthService>();
        controller = new AuthController(authServiceMock.Object);
    }

    #region RegisterAsync

    [Test]
    public async Task Register_WithValidCredentials_ReturnsOk()
    {
        // Arrange
        var credentials = new UserCredentialsDto { Username = "admin", Password = "password123" };
        authServiceMock
            .Setup(s => s.RegisterAsync("admin", "password123"))
            .ReturnsAsync(CreateTestUser());

        // Act
        var result = await controller.Register(credentials);

        // Assert
        Assert.That(result, Is.TypeOf<OkResult>());
    }
    
    [Test]
    public async Task Register_WithValidCredentials_CallsService()
    {
        // Arrange
        var credentials = new UserCredentialsDto { Username = "new_user", Password = "password123" };
        authServiceMock
            .Setup(s => s.RegisterAsync("new_user", "password123"))
            .ReturnsAsync(CreateTestUser());

        // Act
        await controller.Register(credentials);

        // Assert
        authServiceMock.Verify(s => s.RegisterAsync("new_user", "password123"), Times.Once);
    }
    
    [Test]
    public async Task Register_WhenServiceReturnsNull_ReturnsBadRequest()
    {
        // Arrange
        var credentials = new UserCredentialsDto { Username = "existing_user", Password = "password123" };
        authServiceMock
            .Setup(s => s.RegisterAsync("existing_user", "password123"))
            .ReturnsAsync((User?)null);

        // Act
        var result = await controller.Register(credentials);

        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Register_WhenServiceReturnsNull_ReturnsErrorMessage()
    {
        // Arrange
        var credentials = new UserCredentialsDto { Username = "existing_user", Password = "password123" };
        authServiceMock
            .Setup(s => s.RegisterAsync("existing_user", "password123"))
            .ReturnsAsync((User?)null);

        // Act
        var result = await controller.Register(credentials) as BadRequestObjectResult;

        // Assert
        Assert.That(result!.Value, Is.EqualTo("Invalid username or password"));
    }

    #endregion

    #region LoginAsync

    [Test]
    public async Task Login_WithValidCredentials_ReturnsOk()
    {
        // Arrange
        var credentials = new UserCredentialsDto { Username = "test_user", Password = "good_password" };
        authServiceMock
            .Setup(s => s.LoginAsync("test_user", "good_password"))
            .ReturnsAsync(CreateTestUser());
        authServiceMock
            .Setup(s => s.GenerateJwtToken(It.IsAny<User>()))
            .Returns("jwt-token");

        // Act
        var result = await controller.Login(credentials);

        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var credentials = new UserCredentialsDto { Username = "test_user", Password = "good_password" };
        authServiceMock
            .Setup(s => s.LoginAsync("test_user", "good_password"))
            .ReturnsAsync(CreateTestUser());
        authServiceMock
            .Setup(s => s.GenerateJwtToken(It.IsAny<User>()))
            .Returns("jwt-token-12345");

        // Act
        var result = await controller.Login(credentials) as OkObjectResult;

        // Assert
        Assert.That(result!.Value, Is.EqualTo("jwt-token-12345"));
    }

    [Test]
    public async Task Login_WithValidCredentials_GeneratesTokenForCorrectUser()
    {
        // Arrange
        var credentials = new UserCredentialsDto { Username = "test_user", Password = "good_password" };
        var user = CreateTestUser();
        authServiceMock
            .Setup(s => s.LoginAsync("test_user", "good_password"))
            .ReturnsAsync(user);
        authServiceMock
            .Setup(s => s.GenerateJwtToken(It.IsAny<User>()))
            .Returns("token");

        // Act
        await controller.Login(credentials);

        // Assert
        authServiceMock.Verify(s => s.GenerateJwtToken(user), Times.Once);
    }

    [Test]
    public async Task Login_WithInvalidCredentials_ReturnsBadRequest()
    {
        // Arrange
        var credentials = new UserCredentialsDto { Username = "test_user", Password = "wrongpassword" };
        authServiceMock
            .Setup(s => s.LoginAsync("test_user", "wrongpassword"))
            .ReturnsAsync((User?)null);

        // Act
        var result = await controller.Login(credentials);

        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Login_WithInvalidCredentials_DoesNotGenerateToken()
    {
        // Arrange
        var credentials = new UserCredentialsDto { Username = "test_user", Password = "wrongpassword" };
        authServiceMock
            .Setup(s => s.LoginAsync("test_user", "wrongpassword"))
            .ReturnsAsync((User?)null);

        // Act
        await controller.Login(credentials);

        // Assert
        authServiceMock.Verify(s => s.GenerateJwtToken(It.IsAny<User>()), Times.Never);
    }

    #endregion
    
    private static User CreateTestUser() => new()
    {
        Id = 1,
        Username = "admin",
        PasswordHash = "hash",
        Posts = [],
        PostLikes = []
    };
}