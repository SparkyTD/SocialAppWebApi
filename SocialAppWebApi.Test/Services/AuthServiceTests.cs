using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SocialAppWebApi.Data;
using SocialAppWebApi.Services;

namespace SocialAppWebApi.Test.Services;

public class AuthServiceTests
{
    private DatabaseFixture databaseFixture;
    private AuthService authService;
    private TokenValidationParameters tokenValidationParameters;
    
    [SetUp]
    public void Setup()
    {
        databaseFixture = new DatabaseFixture();

        tokenValidationParameters = new TokenValidationParameters
        {
            ValidAudience = "Test",
            ValidIssuer = "Test",
            IssuerSigningKey = new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(128)),
        };
        
        var usersService = new UsersService(databaseFixture.Database);
        authService = new AuthService(databaseFixture.Database, usersService, tokenValidationParameters);
    }

    #region RegisterAsync

    [Test]
    public async Task RegisterAsync_WithValidCredentials_ReturnsUser()
    {
        // Act
        var result = await authService.RegisterAsync("admin", "password123");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Username, Is.EqualTo("admin"));
    }
    
    [Test]
    public async Task RegisterAsync_WithValidCredentials_HashesPassword()
    {
        // Act
        var result = await authService.RegisterAsync("newuser", "password123");

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result!.PasswordHash, Is.Not.EqualTo("password123"));
            Assert.That(BCrypt.Net.BCrypt.Verify("password123", result.PasswordHash), Is.True);
        }
    }
    
    [Test]
    public async Task RegisterAsync_WithValidCredentials_PersistsToDatabase()
    {
        // Act
        await authService.RegisterAsync("admin", "password123");

        // Assert
        var userInDb = await databaseFixture.Database.Users.FirstOrDefaultAsync(u => u.Username == "admin");
        Assert.That(userInDb, Is.Not.Null);
    }
    
    [Test]
    public async Task RegisterAsync_WithDuplicateUsername_ReturnsNull()
    {
        // Arrange
        await authService.RegisterAsync("existing_user", "password123");

        // Act
        var result = await authService.RegisterAsync("existing_user", "different_password");

        // Assert
        Assert.That(result, Is.Null);
    }
    
    [Test]
    public async Task RegisterAsync_WithDuplicateUsername_DoesNotCreateSecondUser()
    {
        await authService.RegisterAsync("existing_user", "password123");
        await authService.RegisterAsync("existing_user", "differentpassword");

        var count = await databaseFixture.Database.Users.CountAsync(u => u.Username == "existing_user");
        Assert.That(count, Is.EqualTo(1));
    }

    #endregion
    
    #region LoginAsync
    
    [Test]
    public async Task LoginAsync_WithValidCredentials_ReturnsUser()
    {
        // Arrange
        await authService.RegisterAsync("admin", "password123");

        // Act
        var result = await authService.LoginAsync("admin", "password123");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Username, Is.EqualTo("admin"));
    }
    
    [Test]
    public async Task LoginAsync_WithWrongPassword_ReturnsNull()
    {
        await authService.RegisterAsync("admin", "password123");

        var result = await authService.LoginAsync("admin", "wrong_password");

        Assert.That(result, Is.Null);
    }
    
    [Test]
    public async Task LoginAsync_WithNonExistentUser_ReturnsNull()
    {
        var result = await authService.LoginAsync("nonexistent", "password");

        Assert.That(result, Is.Null);
    }
    
    #endregion
    
    #region GenerateJwtToken
    
    [Test]
    public void GenerateJwtToken_ReturnsValidTokenString()
    {
        var user = new User
        {
            Id = 42,
            Username = "admin",
            PasswordHash = "hash",
            Posts = [],
            PostLikes = []
        };

        var token = authService.GenerateJwtToken(user);

        Assert.That(token, Is.Not.Null.And.Not.Empty);
    }
    
    [Test]
    public void GenerateJwtToken_ContainsCorrectClaims()
    {
        var user = new User
        {
            Id = 42,
            Username = "admin",
            PasswordHash = "hash",
            Posts = [],
            PostLikes = []
        };

        var token = authService.GenerateJwtToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        Assert.Multiple(() =>
        {
            Assert.That(jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value, Is.EqualTo("42"));
            Assert.That(jwt.Claims.First(c => c.Type == ClaimTypes.Name).Value, Is.EqualTo("admin"));
        });
    }
    
    [Test]
    public void GenerateJwtToken_CanBeValidated()
    {
        var user = new User
        {
            Id = 42,
            Username = "admin",
            PasswordHash = "hash",
            Posts = [],
            PostLikes = []
        };

        var token = authService.GenerateJwtToken(user);
        var handler = new JwtSecurityTokenHandler();

        var principal = handler.ValidateToken(token, tokenValidationParameters, out var validatedToken);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(validatedToken, Is.Not.Null);
            Assert.That(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value, Is.EqualTo("42"));
        }
    }
    
    #endregion

    [TearDown]
    public void TearDown()
    {
        databaseFixture.Dispose();
    }
}