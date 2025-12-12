using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SocialAppWebApi.Data;

namespace SocialAppWebApi.Test;

public class DatabaseFixture : IDisposable
{
    public AppDatabase Database { get; }
    private readonly SqliteConnection connection;

    public DatabaseFixture()
    {
        connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        
        var options = new DbContextOptionsBuilder<AppDatabase>()
            .UseSqlite(connection)
            .EnableSensitiveDataLogging()
            .LogTo(Console.WriteLine)
            .Options;
        
        Database = new AppDatabase(options);
        Database.Database.EnsureCreated();
    }
    
    public void Dispose()
    {
        Database.Dispose();
        connection.Dispose();
    }
}