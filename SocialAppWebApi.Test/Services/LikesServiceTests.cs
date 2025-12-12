using Microsoft.EntityFrameworkCore;
using SocialAppWebApi.Data;
using SocialAppWebApi.Services;

namespace SocialAppWebApi.Test.Services;

public class LikesServiceTests
{
    private DatabaseFixture databaseFixture;
    private LikesService likesService;
    private User testUser = null!;
    private Post testPost = null!;

    [SetUp]
    public async Task Setup()
    {
        databaseFixture = new DatabaseFixture();
        likesService = new LikesService(databaseFixture.Database);
        
        testUser = new User
        {
            Username = "test_user",
            PasswordHash = "hash",
            Posts = [],
            PostLikes = []
        };
        databaseFixture.Database.Users.Add(testUser);
        await databaseFixture.Database.SaveChangesAsync();

        testPost = new Post
        {
            Content = "Test post",
            CreatedAt = DateTime.UtcNow,
            AuthorId = testUser.Id,
            Author = testUser,
            CachedLikeCount = 0,
            Likes = []
        };
        databaseFixture.Database.Posts.Add(testPost);
        await databaseFixture.Database.SaveChangesAsync();
    }

    #region CreateLikeAsync

    [Test]
    public async Task CreateLikeAsync_WithValidInput_ReturnsTrue()
    {
        // Arrange
        // using test user and test post

        // Act
        var result = await likesService.CreateLikeAsync(testUser, testPost.Id);

        // Assert
        Assert.That(result, Is.True);
    }
    
    [Test]
    public async Task CreateLikeAsync_WithValidInput_PersistsLike()
    {
        // Arrange
        // using test user and test post

        // Act
        await likesService.CreateLikeAsync(testUser, testPost.Id);

        // Assert
        var like = await databaseFixture.Database.PostLikes.FirstOrDefaultAsync(
            l => l.PostId == testPost.Id && l.UserId == testUser.Id);
        Assert.That(like, Is.Not.Null);
    }
    
    [Test]
    public async Task CreateLikeAsync_WithValidInput_IncrementsCachedLikeCount()
    {
        // Arrange
        // (using default testUser and testPost)

        // Act
        await likesService.CreateLikeAsync(testUser, testPost.Id);

        // Assert
        await databaseFixture.Database.Entry(testPost).ReloadAsync();
        var post = await databaseFixture.Database.Posts.FindAsync(testPost.Id);
        Assert.That(post!.CachedLikeCount, Is.EqualTo(1));
    }
    
    [Test]
    public async Task CreateLikeAsync_MultipleLikes_IncrementsCachedLikeCountCorrectly()
    {
        // Arrange
        var user2 = await CreateUserAsync("user2");
        var user3 = await CreateUserAsync("user3");

        // Act
        await likesService.CreateLikeAsync(testUser, testPost.Id);
        await likesService.CreateLikeAsync(user2, testPost.Id);
        await likesService.CreateLikeAsync(user3, testPost.Id);

        // Assert
        await databaseFixture.Database.Entry(testPost).ReloadAsync();
        var post = await databaseFixture.Database.Posts.FindAsync(testPost.Id);
        Assert.That(post!.CachedLikeCount, Is.EqualTo(3));
    }
    
    [Test]
    public async Task CreateLikeAsync_DuplicateLike_ReturnsFalse()
    {
        // Arrange
        await likesService.CreateLikeAsync(testUser, testPost.Id);

        // Act
        var result = await likesService.CreateLikeAsync(testUser, testPost.Id);

        // Assert
        Assert.That(result, Is.False);
    }

    #endregion

    #region DeleteLikeAsync

    [Test]
    public async Task DeleteLikeAsync_WithExistingLike_ReturnsTrue()
    {
        // Arrange
        await likesService.CreateLikeAsync(testUser, testPost.Id);

        // Act
        var result = await likesService.DeleteLikeAsync(testUser, testPost.Id);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task DeleteLikeAsync_WithExistingLike_RemovesLike()
    {
        // Arrange
        await likesService.CreateLikeAsync(testUser, testPost.Id);

        // Act
        await likesService.DeleteLikeAsync(testUser, testPost.Id);

        // Assert
        var like = await databaseFixture.Database.PostLikes.FirstOrDefaultAsync(
            l => l.PostId == testPost.Id && l.UserId == testUser.Id);
        Assert.That(like, Is.Null);
    }

    [Test]
    public async Task DeleteLikeAsync_WithExistingLike_DecrementsCachedLikeCount()
    {
        // Arrange
        await likesService.CreateLikeAsync(testUser, testPost.Id);

        // Act
        await likesService.DeleteLikeAsync(testUser, testPost.Id);

        // Assert
        var post = await databaseFixture.Database.Posts.FindAsync(testPost.Id);
        Assert.That(post!.CachedLikeCount, Is.EqualTo(0));
    }

    [Test]
    public async Task DeleteLikeAsync_WithNonExistentLike_ReturnsFalse()
    {
        // Arrange
        // (no like created)

        // Act
        var result = await likesService.DeleteLikeAsync(testUser, testPost.Id);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task DeleteLikeAsync_CountNeverGoesBelowZero()
    {
        // Arrange
        // (no like created, count is 0)

        // Act
        await likesService.DeleteLikeAsync(testUser, testPost.Id);

        // Assert
        var post = await databaseFixture.Database.Posts.FindAsync(testPost.Id);
        Assert.That(post!.CachedLikeCount, Is.EqualTo(0));
    }

    [Test]
    public async Task DeleteLikeAsync_OnlyRemovesSpecifiedUserLike()
    {
        // Arrange
        var user2 = await CreateUserAsync("user2");
        await likesService.CreateLikeAsync(testUser, testPost.Id);
        await likesService.CreateLikeAsync(user2, testPost.Id);

        // Act
        await likesService.DeleteLikeAsync(testUser, testPost.Id);

        // Assert
        await databaseFixture.Database.Entry(testPost).ReloadAsync();
        var remainingLike = await databaseFixture.Database.PostLikes.FirstOrDefaultAsync(
            l => l.PostId == testPost.Id && l.UserId == user2.Id);
        Assert.That(remainingLike, Is.Not.Null);
        
        var post = await databaseFixture.Database.Posts.FindAsync(testPost.Id);
        Assert.That(post!.CachedLikeCount, Is.EqualTo(1));
    }

    #endregion

    #region UpdateAllCachedLikeCountsAsync

    [Test]
    public async Task UpdateAllCachedLikeCountsAsync_ReconcilesCounts()
    {
        // Arrange
        await likesService.CreateLikeAsync(testUser, testPost.Id);
        await databaseFixture.Database.Posts
            .Where(p => p.Id == testPost.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.CachedLikeCount, 999));

        // Act
        await likesService.UpdateAllCachedLikeCountsAsync();

        // Assert
        await databaseFixture.Database.Entry(testPost).ReloadAsync();
        var post = await databaseFixture.Database.Posts.FindAsync(testPost.Id);
        Assert.That(post!.CachedLikeCount, Is.EqualTo(1));
    }

    #endregion
    
    private async Task<User> CreateUserAsync(string username)
    {
        var user = new User
        {
            Username = username,
            PasswordHash = "hash",
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