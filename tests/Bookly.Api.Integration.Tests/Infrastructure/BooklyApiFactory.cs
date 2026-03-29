using Bookly.Core.Data;
using Bookly.Core.Services;
using Bookly.Core.UseCases;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Bookly.Api.Integration.Tests.Infrastructure;

/// <summary>
/// Custom WebApplicationFactory that replaces the Aspire-registered DbContext
/// with one pointing at the TestContainers PostgreSQL instance and stubs
/// external metadata providers so tests run without network access.
/// </summary>
public sealed class BooklyApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public BooklyApiFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            // Remove ALL registrations related to BooklyDbContext
            // (Aspire registers pooling, options, leases, etc.)
            var dbContextDescriptors = services
                .Where(d =>
                    d.ServiceType.FullName?.Contains(nameof(BooklyDbContext)) == true
                    || d.ImplementationType?.FullName?.Contains(nameof(BooklyDbContext)) == true)
                .ToList();

            foreach (var descriptor in dbContextDescriptors)
                services.Remove(descriptor);

            // Also remove generic DbContextOptions if somehow leftover
            services.RemoveAll<DbContextOptions<BooklyDbContext>>();

            // Register DbContext against the test PostgreSQL container (no pooling)
            services.AddDbContext<BooklyDbContext>(options =>
                options.UseNpgsql(_connectionString));

            // Replace external metadata providers with a stub
            services.RemoveAll<IBookMetadataProvider>();
            services.RemoveAll<OpenLibraryProvider>();
            services.RemoveAll<GoogleBooksProvider>();
            services.RemoveAll<BookLookupOrchestrator>();
            services.RemoveAll<IIsbnMetadataService>();
            services.RemoveAll<IBookRepository>();
            services.RemoveAll<IAuthorRepository>();
            services.RemoveAll<IAddBookUseCase>();
            services.RemoveAll<IListBooksUseCase>();

            services.AddTransient<IBookMetadataProvider, StubMetadataProvider>();
            services.AddScoped<BookLookupOrchestrator>();
            services.AddScoped<IIsbnMetadataService, IsbnMetadataService>();
            services.AddScoped<IBookRepository, BookRepository>();
            services.AddScoped<IAuthorRepository, AuthorRepository>();
            services.AddScoped<IAddBookUseCase, AddBookUseCase>();
            services.AddScoped<IListBooksUseCase, ListBooksUseCase>();
        });
    }

    /// <summary>
    /// Applies EF Core migrations against the test database.
    /// Call once after container is started, before running tests.
    /// </summary>
    public async Task MigrateDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BooklyDbContext>();
        await db.Database.MigrateAsync();
    }

    /// <summary>
    /// Creates a new scope and returns the DbContext for direct DB verification.
    /// Caller is responsible for disposing the scope.
    /// </summary>
    public (IServiceScope Scope, BooklyDbContext Db) CreateDbScope()
    {
        var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BooklyDbContext>();
        return (scope, db);
    }
}
