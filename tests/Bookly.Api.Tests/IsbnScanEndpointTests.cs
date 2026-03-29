using Bookly.Api.Endpoints;
using Bookly.Api.Models;
using Bookly.Api.Services;
using Bookly.Core.Data;
using Bookly.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bookly.Api.Tests;

public class IsbnScanEndpointTests : IDisposable
{
    private readonly BooklyDbContext _db;

    public IsbnScanEndpointTests()
    {
        var options = new DbContextOptionsBuilder<BooklyDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _db = new BooklyDbContext(options);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task HandleScanAsync_ValidIsbn_CreatesBook()
    {
        var orchestrator = CreateOrchestrator(new BookMetadata
        {
            Title = "Test Book",
            Authors = ["Author One", "Author Two"],
            Publisher = "Test Publisher",
            PageCount = 200,
            Source = "TestSource"
        });

        var request = new IsbnScanRequest { Isbn = "978-0-306-40615-7" };

        var result = await IsbnScanEndpoint.HandleScanAsync(
            request, orchestrator, _db,
            NullLogger<BookLookupOrchestrator>.Instance,
            CancellationToken.None);

        var created = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.Created<BookDto>>(result.Result);
        Assert.Equal("Test Book", created.Value!.Title);
        Assert.Equal("9780306406157", created.Value.NormalizedIsbn);
        Assert.Equal(2, created.Value.Authors.Count);
        Assert.Equal("TestSource", created.Value.MetadataSource);

        // Verify persisted
        var book = await _db.Books.Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .FirstAsync();
        Assert.Equal("Test Book", book.Title);
        Assert.Equal(2, book.BookAuthors.Count);
    }

    [Fact]
    public async Task HandleScanAsync_DuplicateIsbn_ReturnsExistingBook()
    {
        // Seed an existing book
        var existing = new Book
        {
            NormalizedIsbn = "9780306406157",
            Title = "Existing Book",
            MetadataSource = "Manual",
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        _db.Books.Add(existing);
        await _db.SaveChangesAsync();

        var orchestrator = CreateOrchestrator(new BookMetadata
        {
            Title = "Should Not Be Used",
            Source = "TestSource"
        });

        var request = new IsbnScanRequest { Isbn = "978-0-306-40615-7" };

        var result = await IsbnScanEndpoint.HandleScanAsync(
            request, orchestrator, _db,
            NullLogger<BookLookupOrchestrator>.Instance,
            CancellationToken.None);

        var ok = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.Ok<BookDto>>(result.Result);
        Assert.Equal("Existing Book", ok.Value!.Title);

        // Verify no duplicate created
        Assert.Equal(1, await _db.Books.CountAsync());
    }

    [Fact]
    public async Task HandleScanAsync_InvalidIsbn_ReturnsValidationProblem()
    {
        var orchestrator = CreateOrchestrator(null);
        var request = new IsbnScanRequest { Isbn = "invalid" };

        var result = await IsbnScanEndpoint.HandleScanAsync(
            request, orchestrator, _db,
            NullLogger<BookLookupOrchestrator>.Instance,
            CancellationToken.None);

        Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.ValidationProblem>(result.Result);
    }

    [Fact]
    public async Task HandleScanAsync_NotFound_ReturnsProblem()
    {
        var orchestrator = CreateOrchestrator(null);
        var request = new IsbnScanRequest { Isbn = "978-0-306-40615-7" };

        var result = await IsbnScanEndpoint.HandleScanAsync(
            request, orchestrator, _db,
            NullLogger<BookLookupOrchestrator>.Instance,
            CancellationToken.None);

        var problem = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult>(result.Result);
        Assert.Equal(404, problem.StatusCode);
    }

    private static BookLookupOrchestrator CreateOrchestrator(BookMetadata? metadata)
    {
        var provider = new InMemoryProvider(metadata);
        return new BookLookupOrchestrator(
            [provider],
            NullLogger<BookLookupOrchestrator>.Instance);
    }

    private sealed class InMemoryProvider(BookMetadata? result) : IBookMetadataProvider
    {
        public string SourceName => "InMemory";
        public Task<BookMetadata?> LookupAsync(string isbn, CancellationToken cancellationToken = default)
            => Task.FromResult(result);
    }
}
