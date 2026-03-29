using Bookly.Core.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Bookly.Cli.Tests.Fixtures;

public sealed class PostgresqlFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _container;
    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        _container = new PostgreSqlBuilder("postgres:16-alpine")
            .WithDatabase("bookly_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithCleanUp(true)
            .Build();

        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString();

        var options = new DbContextOptionsBuilder<BooklyDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        await using var dbContext = new BooklyDbContext(options);
        await dbContext.Database.MigrateAsync();
    }

    public BooklyDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BooklyDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        return new BooklyDbContext(options);
    }

    public async Task CleanAsync()
    {
        await using var db = CreateDbContext();
        db.BookAuthors.RemoveRange(db.BookAuthors);
        db.Books.RemoveRange(db.Books);
        db.Authors.RemoveRange(db.Authors);
        await db.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.StopAsync();
            await _container.DisposeAsync();
        }
    }
}

[CollectionDefinition(nameof(PostgresqlFixtureCollection))]
public sealed class PostgresqlFixtureCollection : ICollectionFixture<PostgresqlFixture>;
