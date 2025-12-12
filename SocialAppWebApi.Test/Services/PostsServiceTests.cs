using Microsoft.EntityFrameworkCore;
using SocialAppWebApi.Data;
using SocialAppWebApi.Services;

namespace SocialAppWebApi.Test.Services;

public class PostsServiceTests
{
    private DatabaseFixture databaseFixture;
    private PostsService postsService;
    private User testUser = null!;
    
    [SetUp]
    public async Task Setup()
    {
        databaseFixture = new DatabaseFixture();
        postsService = new PostsService(databaseFixture.Database);
        
        testUser = new User
        {
            Username = "test_user",
            PasswordHash = "hash",
            Posts = [],
            PostLikes = []
        };
        databaseFixture.Database.Users.Add(testUser);
        await databaseFixture.Database.SaveChangesAsync();
    }

    #region GetPosts

    [Test]
    public async Task GetPosts_WithPosts_ReturnsPostsInReverseChronologicalOrder()
    {
        // Arrange
        await CreatePostAsync("First", DateTime.UtcNow.AddHours(-2));
        await CreatePostAsync("Second", DateTime.UtcNow.AddHours(-1));
        await CreatePostAsync("Third", DateTime.UtcNow);

        // Act
        var results = postsService.GetPosts(page: 1, pageSize: 10).ToList();

        // Assert
        Assert.That(results.Select(p => p.Content), Is.EqualTo(new[] { "Third", "Second", "First" }));
    }
    
    [Test]
    public async Task GetPosts_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        for (int i = 1; i <= 5; i++)
            await CreatePostAsync($"Post {i}", DateTime.UtcNow.AddMinutes(i));

        // Act
        var page1 = postsService.GetPosts(page: 1, pageSize: 2).ToList();
        var page2 = postsService.GetPosts(page: 2, pageSize: 2).ToList();
        var page3 = postsService.GetPosts(page: 3, pageSize: 2).ToList();

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(page1.Select(p => p.Content), Is.EqualTo(new[] { "Post 5", "Post 4" }));
            Assert.That(page2.Select(p => p.Content), Is.EqualTo(new[] { "Post 3", "Post 2" }));
            Assert.That(page3.Select(p => p.Content), Is.EqualTo(new[] { "Post 1" }));
        }
    }
    
    [Test]
    public async Task GetPosts_IncludesAuthor()
    {
        // Arrange
        await CreatePostAsync("Test post", DateTime.UtcNow);

        // Act
        var result = postsService.GetPosts(page: 1, pageSize: 10).First();

        // Assert
        Assert.That(result.Author, Is.Not.Null);
        Assert.That(result.Author.Username, Is.EqualTo("test_user"));
    }
    
    [Test]
    public async Task GetPosts_PageBeyondData_ReturnsEmptyCollection()
    {
        // Arrange
        await CreatePostAsync("Only post", DateTime.UtcNow);

        // Act
        var results = postsService.GetPosts(page: 99, pageSize: 10);

        // Assert
        Assert.That(results, Is.Empty);
    }

    #endregion
    
    #region GetPostByIdAsync
    
    [Test]
    public async Task GetPostByIdAsync_WithExistingPost_ReturnsPost()
    {
        // Arrange
        var post = await CreatePostAsync("Test content", DateTime.UtcNow);

        // Act
        var result = await postsService.GetPostByIdAsync(post.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Content, Is.EqualTo("Test content"));
    }
    
    [Test]
    public async Task GetPostByIdAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        const long nonExistentId = 99999L;

        // Act
        var result = await postsService.GetPostByIdAsync(nonExistentId);

        // Assert
        Assert.That(result, Is.Null);
    }
    
    #endregion
    
    #region SavePostAsync 
    
    [Test]
    public async Task SavePostAsync_WithValidPost_PersistsToDatabase()
    {
        // Arrange
        var post = new Post
        {
            Content = "Hello World!",
            CreatedAt = DateTime.UtcNow,
            AuthorId = testUser.Id,
            Author = testUser,
            CachedLikeCount = 0,
            Likes = []
        };

        // Act
        await postsService.SavePostAsync(post);

        // Assert
        var savedPost = await databaseFixture.Database.Posts.FirstOrDefaultAsync(p => p.Content == "Hello World!");
        Assert.That(savedPost, Is.Not.Null);
    }
    
    [Test]
    public async Task SavePostAsync_ReturnsPostWithGeneratedId()
    {
        // Arrange
        var post = new Post
        {
            Content = "New post",
            CreatedAt = DateTime.UtcNow,
            AuthorId = testUser.Id,
            Author = testUser,
            Likes = []
        };

        // Act
        var result = await postsService.SavePostAsync(post);

        // Assert
        Assert.That(result.Id, Is.GreaterThan(0));
    }
    
    #endregion

    private async Task<Post> CreatePostAsync(string content, DateTime createdAt)
    {
        var post = new Post
        {
            Content = content,
            CreatedAt = createdAt,
            AuthorId = testUser.Id,
            Author = testUser,
            Likes = []
        };
        databaseFixture.Database.Posts.Add(post);
        await databaseFixture.Database.SaveChangesAsync();
        return post;
    }
    
    [TearDown]
    public void TearDown()
    {
        databaseFixture.Dispose();
    }
}