using Testcontainers.PostgreSql;

namespace Bookly.Api.Integration.Tests.Infrastructure;

/// <summary>
/// Shared PostgreSQL container for all integration tests in a collection.
/// Implements IAsyncLifetime so xUnit manages startup/teardown automatically.
/// </summary>
public sealed class PostgresContainer : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("bookly_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync().AsTask();
    }
}
