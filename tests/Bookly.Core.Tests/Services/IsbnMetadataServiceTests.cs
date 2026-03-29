using System.Net;
using Bookly.Core.Services;
using Bookly.Core.Tests.Fixtures;

namespace Bookly.Core.Tests.Services;

public class IsbnMetadataServiceTests
{
    private static HttpClient CreateClient(HttpStatusCode statusCode, string content)
    {
        var handler = new FakeHttpMessageHandler(statusCode, content);
        return new HttpClient(handler);
    }

    [Fact]
    public async Task ResolveIsbn_ValidIsbn_FetchesMetadata()
    {
        var openLibraryResponse = $$"""
        {
          "ISBN:{{TestData.ValidIsbn13}}": {
            "title": "Test Book",
            "authors": [{"name": "Author One"}],
            "publishers": [{"name": "Test Publisher"}]
          }
        }
        """;

        var client = CreateClient(HttpStatusCode.OK, openLibraryResponse);
        var service = new IsbnMetadataService(client);

        var result = await service.ResolveIsbnAsync(TestData.ValidIsbn13);

        Assert.NotNull(result);
        Assert.Equal("Test Book", result.Title);
        Assert.Contains("Author One", result.Authors);
    }

    [Fact]
    public async Task ResolveIsbn_ApiTimeout_UsesFallback()
    {
        var handler = new ThrowingHttpMessageHandler(new HttpRequestException("Timeout"));
        var client = new HttpClient(handler);
        var service = new IsbnMetadataService(client, enableFallback: true);

        var result = await service.ResolveIsbnAsync(TestData.ValidIsbn13);

        Assert.NotNull(result);
        Assert.Equal("[Fallback] Unknown Book", result.Title);
        Assert.Equal("fallback", result.Source);
    }

    [Fact]
    public async Task ResolveIsbn_ApiTimeout_NoFallback_ReturnsNull()
    {
        var handler = new ThrowingHttpMessageHandler(new HttpRequestException("Timeout"));
        var client = new HttpClient(handler);
        var service = new IsbnMetadataService(client, enableFallback: false);

        var result = await service.ResolveIsbnAsync(TestData.ValidIsbn13);

        Assert.Null(result);
    }

    [Theory]
    [InlineData("not-an-isbn")]
    [InlineData("123")]
    [InlineData("")]
    public async Task ResolveIsbn_InvalidIsbn_ReturnsNull(string isbn)
    {
        var client = CreateClient(HttpStatusCode.OK, "{}");
        var service = new IsbnMetadataService(client);

        var result = await service.ResolveIsbnAsync(isbn);

        Assert.Null(result);
    }

    [Fact]
    public async Task ResolveIsbn_EmptyResponse_ReturnsFallbackOrNull()
    {
        var client = CreateClient(HttpStatusCode.OK, "{}");
        var service = new IsbnMetadataService(client, enableFallback: true);

        var result = await service.ResolveIsbnAsync(TestData.ValidIsbn13);

        // Empty JSON = no key found = fallback
        Assert.NotNull(result);
        Assert.Equal("fallback", result.Source);
    }
}

internal class FakeHttpMessageHandler(HttpStatusCode statusCode, string content) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => Task.FromResult(new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content)
        });
}

internal class ThrowingHttpMessageHandler(Exception exception) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => Task.FromException<HttpResponseMessage>(exception);
}
