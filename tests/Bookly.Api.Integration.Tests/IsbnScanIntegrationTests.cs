using System.Net;
using System.Net.Http.Json;
using Bookly.Api.Integration.Tests.Infrastructure;
using Bookly.Api.Models;
using Bookly.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Bookly.Api.Integration.Tests;

[Collection(IntegrationTestCollection.Name)]
public sealed class IsbnScanIntegrationTests : IAsyncLifetime
{
    private readonly PostgresContainer _postgres;
    private BooklyApiFactory _factory = null!;
    private HttpClient _client = null!;

    public IsbnScanIntegrationTests(PostgresContainer postgres)
    {
        _postgres = postgres;
    }

    public async Task InitializeAsync()
    {
        _factory = new BooklyApiFactory(_postgres.ConnectionString);
        _client = _factory.CreateClient();
        await _factory.MigrateDatabaseAsync();

        // Clean up any leftover data from previous tests
        var (scope, db) = _factory.CreateDbScope();
        await using (scope as IAsyncDisposable)
        {
            db.BookAuthors.RemoveRange(db.BookAuthors);
            db.Books.RemoveRange(db.Books);
            db.Authors.RemoveRange(db.Authors);
            await db.SaveChangesAsync();
        }
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task AddBookWithIsbnAndVerifyInDatabase()
    {
        // Arrange — valid ISBN-13 for a well-known book
        const string testIsbn = "9780134685991";
        var request = new IsbnScanRequest { Isbn = testIsbn };

        // Act — POST to the isbn-scan endpoint
        var response = await _client.PostAsJsonAsync("/api/library/isbn-scan", request);

        // Assert 1: HTTP response is 201 Created
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        // Assert 2: Response body contains correct book data
        var bookDto = await response.Content.ReadFromJsonAsync<BookDto>();
        Assert.NotNull(bookDto);
        Assert.True(bookDto.Id > 0);
        Assert.Equal(testIsbn, bookDto.NormalizedIsbn);
        Assert.Equal($"Test Book for {testIsbn}", bookDto.Title);
        Assert.Equal("A Stub Subtitle", bookDto.Subtitle);
        Assert.Single(bookDto.Authors);
        Assert.Equal("Test Author", bookDto.Authors[0]);
        Assert.Equal("Test Publisher", bookDto.Publisher);
        Assert.Equal(new DateOnly(2024, 1, 15), bookDto.PublishedOn);
        Assert.Equal("en", bookDto.Language);
        Assert.Equal(320, bookDto.PageCount);
        Assert.Equal("A book created by the test stub provider.", bookDto.Description);
        Assert.Equal("Stub", bookDto.MetadataSource);

        // Assert 3: Verify the book is persisted in the real database
        var (scope, db) = _factory.CreateDbScope();
        await using (scope as IAsyncDisposable)
        {
            var persisted = await db.Books
                .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
                .FirstOrDefaultAsync(b => b.NormalizedIsbn == testIsbn);

            Assert.NotNull(persisted);
            Assert.Equal(testIsbn, persisted.NormalizedIsbn);
            Assert.Equal($"Test Book for {testIsbn}", persisted.Title);
            Assert.Equal("A Stub Subtitle", persisted.Subtitle);
            Assert.Equal("Test Publisher", persisted.Publisher);
            Assert.Equal(new DateOnly(2024, 1, 15), persisted.PublishedOn);
            Assert.Equal("en", persisted.Language);
            Assert.Equal(320, persisted.PageCount);
            Assert.Equal("A book created by the test stub provider.", persisted.Description);
            Assert.Equal("Stub", persisted.MetadataSource);

            // Verify author relationship
            Assert.Single(persisted.BookAuthors);
            Assert.Equal("Test Author", persisted.BookAuthors.First().Author.Name);
        }
    }

    [Fact]
    public async Task ScanSameIsbnTwice_ReturnsExistingBook()
    {
        // Arrange
        const string testIsbn = "9780201633610";
        var request = new IsbnScanRequest { Isbn = testIsbn };

        // Act — first scan creates the book
        var firstResponse = await _client.PostAsJsonAsync("/api/library/isbn-scan", request);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        var firstBook = await firstResponse.Content.ReadFromJsonAsync<BookDto>();
        Assert.NotNull(firstBook);

        // Act — second scan returns existing
        var secondResponse = await _client.PostAsJsonAsync("/api/library/isbn-scan", request);
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);

        var secondBook = await secondResponse.Content.ReadFromJsonAsync<BookDto>();
        Assert.NotNull(secondBook);

        // Assert — same book ID, no duplicate
        Assert.Equal(firstBook.Id, secondBook.Id);

        // Verify only one record in DB
        var (scope, db) = _factory.CreateDbScope();
        await using (scope as IAsyncDisposable)
        {
            var count = await db.Books.CountAsync(b => b.NormalizedIsbn == testIsbn);
            Assert.Equal(1, count);
        }
    }

    [Fact]
    public async Task InvalidIsbn_ReturnsValidationProblem()
    {
        // Arrange — clearly invalid ISBN
        var request = new IsbnScanRequest { Isbn = "invalid" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/library/isbn-scan", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
