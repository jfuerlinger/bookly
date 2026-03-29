using System.Net;
using System.Net.Http.Json;
using Bookly.Api.Integration.Tests.Infrastructure;
using Bookly.Core.Models;

namespace Bookly.Api.Integration.Tests;

[Collection(IntegrationTestCollection.Name)]
public sealed class ListBooksIntegrationTests : IAsyncLifetime
{
    private readonly PostgresContainer _postgres;
    private BooklyApiFactory _factory = null!;
    private HttpClient _client = null!;

    public ListBooksIntegrationTests(PostgresContainer postgres)
    {
        _postgres = postgres;
    }

    public async Task InitializeAsync()
    {
        _factory = new BooklyApiFactory(_postgres.ConnectionString);
        _client = _factory.CreateClient();
        await _factory.MigrateDatabaseAsync();

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
    public async Task GetBooks_EmptyLibrary_ReturnsEmptyArray()
    {
        var response = await _client.GetAsync("/api/library/books");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var books = await response.Content.ReadFromJsonAsync<List<BookDto>>();
        Assert.NotNull(books);
        Assert.Empty(books);
    }

    [Fact]
    public async Task GetBooks_AfterAddingBook_ReturnsBook()
    {
        // Add a book first via the scan endpoint
        var scanResponse = await _client.PostAsJsonAsync("/api/library/isbn-scan",
            new { Isbn = "9780134685991" });
        Assert.Equal(HttpStatusCode.Created, scanResponse.StatusCode);

        // List books
        var response = await _client.GetAsync("/api/library/books");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var books = await response.Content.ReadFromJsonAsync<List<BookDto>>();
        Assert.NotNull(books);
        Assert.Single(books);
        Assert.Equal("9780134685991", books[0].NormalizedIsbn);
    }

    [Fact]
    public async Task GetBooks_Pagination_RespectsSkipAndTake()
    {
        // Add two books
        await _client.PostAsJsonAsync("/api/library/isbn-scan", new { Isbn = "9780134685991" });
        await _client.PostAsJsonAsync("/api/library/isbn-scan", new { Isbn = "9780306406157" });

        var firstPage = await _client.GetFromJsonAsync<List<BookDto>>("/api/library/books?skip=0&take=1");
        var secondPage = await _client.GetFromJsonAsync<List<BookDto>>("/api/library/books?skip=1&take=1");
        var allBooks = await _client.GetFromJsonAsync<List<BookDto>>("/api/library/books?skip=0&take=10");

        Assert.NotNull(firstPage);
        Assert.NotNull(secondPage);
        Assert.NotNull(allBooks);
        Assert.Single(firstPage);
        Assert.Single(secondPage);
        Assert.Equal(2, allBooks.Count);
        Assert.NotEqual(firstPage[0].NormalizedIsbn, secondPage[0].NormalizedIsbn);
    }
}
