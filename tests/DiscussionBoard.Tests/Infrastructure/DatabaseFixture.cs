using DiscussionBoard.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DiscussionBoard.Tests.Infrastructure;

public sealed class DatabaseFixture : IAsyncDisposable
{
    private readonly SqliteConnection _connection;

    public DbContextOptions<ApplicationDbContext> Options { get; }

    private DatabaseFixture(SqliteConnection connection, DbContextOptions<ApplicationDbContext> options)
    {
        _connection = connection;
        Options = options;
    }

    public static async Task<DatabaseFixture> CreateAsync()
    {
        SqliteConnection connection = new("Data Source=:memory:");
        await connection.OpenAsync();

        DbContextOptions<ApplicationDbContext> options =
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

        DatabaseFixture fixture = new(connection, options);

        await using ApplicationDbContext context = fixture.CreateContext();
        await context.Database.EnsureCreatedAsync();

        return fixture;
    }

    public ApplicationDbContext CreateContext()
    {
        return new ApplicationDbContext(Options);
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
