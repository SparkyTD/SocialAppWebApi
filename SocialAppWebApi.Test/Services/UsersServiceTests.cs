using SocialAppWebApi.Data;
using SocialAppWebApi.Services;

namespace SocialAppWebApi.Test.Services;

public class UsersServiceTests
{
    private DatabaseFixture databaseFixture;
    private UsersService usersService;
    
    [SetUp]
    public void Setup()
    {
        databaseFixture = new DatabaseFixture();
        usersService = new UsersService(databaseFixture.Database);
    }

    #region GetUserByIdAsync

    [Test]
    public async Task GetUserByIdAsync_WithExistingUser_ReturnsUser()
    {
        // Arrange
        var user = await CreateUserAsync("test", "password_hash");
        
        // Act
        var result = await usersService.GetUserByIdAsync(user.Id);
        
        // Assert
        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Id, Is.EqualTo(user.Id));
            Assert.That(result.Username, Is.EqualTo("test"));
            Assert.That(result.PasswordHash, Is.EqualTo("password_hash"));
        }
    }

    [Test]
    public async Task GetUserByIdAsync_WithNonExistingUser_ReturnsNull()
    {
        // Arrange
        var nonExistentId = 123456;
        
        // Act
        var result = await usersService.GetUserByIdAsync(nonExistentId);
        
        // Assert
        Assert.That(result, Is.Null);
    }
    
    [Test]
    public async Task GetUserByIdAsync_WithNegativeId_ReturnsNull()
    {
        // Arrange
        var nonExistentId = -1;
        
        // Act
        var result = await usersService.GetUserByIdAsync(nonExistentId);

        // Assert
        Assert.That(result, Is.Null);
    }
    
    [Test]
    public async Task GetUserByIdAsync_WithMultipleUsers_ReturnsCorrectUser()
    {
        // Arrange
        await CreateUserAsync("user1", "hash1");
        var user2 = await CreateUserAsync("user2", "hash2");
        await CreateUserAsync("user3", "hash3");

        // Act
        var result = await usersService.GetUserByIdAsync(user2.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Username, Is.EqualTo("user2"));
    }

    #endregion

    #region GetUserByNameAsync

    [Test]
    public async Task GetUserByNameAsync_WithExistingUsername_ReturnsUser()
    {
        // Arrange
        var user = await CreateUserAsync("test_user", "hashed_password");

        // Act
        var result = await usersService.GetUserByNameAsync("test_user");

        // Assert
        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result!.Id, Is.EqualTo(user.Id));
            Assert.That(result.Username, Is.EqualTo("test_user"));
        }
    }
    
    [Test]
    public async Task GetUserByNameAsync_WithNonExistentUsername_ReturnsNull()
    {
        // Arrange
        // none needed
        
        // Act
        var result = await usersService.GetUserByNameAsync("invalid_user");

        // Assert
        Assert.That(result, Is.Null);
    }
    
    [Test]
    public async Task GetUserByNameAsync_IsCaseSensitive()
    {
        // Arrange
        await CreateUserAsync("TestUser", "hashedpassword");

        // Act
        var exactMatch = await usersService.GetUserByNameAsync("TestUser");
        var lowerCase = await usersService.GetUserByNameAsync("testuser");
        var upperCase = await usersService.GetUserByNameAsync("TESTUSER");

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(exactMatch, Is.Not.Null);
            Assert.That(lowerCase, Is.Null);
            Assert.That(upperCase, Is.Null);
        }
    }
    
    [Test]
    public async Task GetUserByNameAsync_WithMultipleUsers_ReturnsCorrectUser()
    {
        // Arrange
        await CreateUserAsync("alice", "hash1");
        var bob = await CreateUserAsync("bob", "hash2");
        await CreateUserAsync("charlie", "hash3");

        // Act
        var result = await usersService.GetUserByNameAsync("bob");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(bob.Id));
    }

    #endregion
    
    private async Task<User> CreateUserAsync(string username, string passwordHash)
    {
        var user = new User
        {
            Username = username,
            PasswordHash = passwordHash,
            Posts = [],
            PostLikes = []
        };

        databaseFixture.Database.Users.Add(user);
        await databaseFixture.Database.SaveChangesAsync();
        return user;
    }

    [TearDown]
    public void TearDown()
    {
        databaseFixture.Dispose();
    }
}