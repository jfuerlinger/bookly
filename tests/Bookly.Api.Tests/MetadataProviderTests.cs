using System.Net;
using System.Text.Json;
using Bookly.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bookly.Api.Tests;

public class OpenLibraryProviderTests
{
    [Fact]
    public async Task LookupAsync_ValidResponse_ReturnsMappedMetadata()
    {
        var isbn = "9780140449136";
        var json = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            [$"ISBN:{isbn}"] = new
            {
                title = "The Republic",
                subtitle = "A Dialogue on Justice",
                authors = new[] { new { name = "Plato" } },
                publishers = new[] { new { name = "Penguin Classics" } },
                publish_date = "October 1, 2007",
                number_of_pages = 416,
                cover = new
                {
                    small = "https://covers.openlibrary.org/s.jpg",
                    medium = "https://covers.openlibrary.org/m.jpg",
                    large = "https://covers.openlibrary.org/l.jpg"
                },
                identifiers = new
                {
                    isbn_10 = new[] { "0140449132" },
                    isbn_13 = new[] { isbn }
                }
            }
        });

        var handler = new FakeHttpHandler(json);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://openlibrary.org") };
        var provider = new OpenLibraryProvider(httpClient, NullLogger<OpenLibraryProvider>.Instance);

        var result = await provider.LookupAsync(isbn);

        Assert.NotNull(result);
        Assert.Equal("The Republic", result.Title);
        Assert.Equal("A Dialogue on Justice", result.Subtitle);
        Assert.Single(result.Authors);
        Assert.Equal("Plato", result.Authors[0]);
        Assert.Equal("Penguin Classics", result.Publisher);
        Assert.Equal(416, result.PageCount);
        Assert.Equal("OpenLibrary", result.Source);
        Assert.Equal("0140449132", result.Isbn10);
        Assert.Equal(isbn, result.Isbn13);
        Assert.NotNull(result.CoverSmallUrl);
        Assert.NotNull(result.CoverMediumUrl);
        Assert.NotNull(result.CoverLargeUrl);
    }

    [Fact]
    public async Task LookupAsync_EmptyResponse_ReturnsNull()
    {
        var handler = new FakeHttpHandler("{}");
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://openlibrary.org") };
        var provider = new OpenLibraryProvider(httpClient, NullLogger<OpenLibraryProvider>.Instance);

        var result = await provider.LookupAsync("0000000000");

        Assert.Null(result);
    }
}

public class GoogleBooksProviderTests
{
    [Fact]
    public async Task LookupAsync_ValidResponse_ReturnsMappedMetadata()
    {
        var isbn = "9780140449136";
        var json = JsonSerializer.Serialize(new
        {
            totalItems = 1,
            items = new[]
            {
                new
                {
                    volumeInfo = new
                    {
                        title = "The Republic",
                        subtitle = "A New Translation",
                        authors = new[] { "Plato" },
                        publisher = "Penguin",
                        publishedDate = "2007-10-01",
                        language = "en",
                        pageCount = 416,
                        description = "A classic work of philosophy.",
                        industryIdentifiers = new[]
                        {
                            new { type = "ISBN_10", identifier = "0140449132" },
                            new { type = "ISBN_13", identifier = isbn }
                        },
                        imageLinks = new
                        {
                            smallThumbnail = "https://books.google.com/s.jpg",
                            thumbnail = "https://books.google.com/m.jpg"
                        }
                    }
                }
            }
        });

        var handler = new FakeHttpHandler(json);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://www.googleapis.com") };
        var provider = new GoogleBooksProvider(httpClient, NullLogger<GoogleBooksProvider>.Instance);

        var result = await provider.LookupAsync(isbn);

        Assert.NotNull(result);
        Assert.Equal("The Republic", result.Title);
        Assert.Equal("A New Translation", result.Subtitle);
        Assert.Single(result.Authors);
        Assert.Equal("Plato", result.Authors[0]);
        Assert.Equal("Penguin", result.Publisher);
        Assert.Equal(new DateOnly(2007, 10, 1), result.PublishedOn);
        Assert.Equal("en", result.Language);
        Assert.Equal(416, result.PageCount);
        Assert.Equal("A classic work of philosophy.", result.Description);
        Assert.Equal("GoogleBooks", result.Source);
        Assert.Equal("0140449132", result.Isbn10);
        Assert.Equal(isbn, result.Isbn13);
    }

    [Fact]
    public async Task LookupAsync_ZeroItems_ReturnsNull()
    {
        var json = JsonSerializer.Serialize(new { totalItems = 0 });
        var handler = new FakeHttpHandler(json);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://www.googleapis.com") };
        var provider = new GoogleBooksProvider(httpClient, NullLogger<GoogleBooksProvider>.Instance);

        var result = await provider.LookupAsync("0000000000");

        Assert.Null(result);
    }
}

/// <summary>
/// Simple fake HTTP handler for unit testing.
/// </summary>
internal class FakeHttpHandler : HttpMessageHandler
{
    private readonly string _responseBody;
    private readonly HttpStatusCode _statusCode;

    public FakeHttpHandler(string responseBody, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _responseBody = responseBody;
        _statusCode = statusCode;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_responseBody, System.Text.Encoding.UTF8, "application/json"),
        });
    }
}
