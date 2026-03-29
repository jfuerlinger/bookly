using Bookly.Core.Models;
using Bookly.Core.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bookly.Core.Tests.Services;

public class IsbnMetadataServiceTests
{
    [Fact]
    public async Task ResolveIsbnAsync_DelegatesToOrchestrator()
    {
        var expected = new BookMetadata { Title = "Test Book", Source = "TestProvider" };
        var provider = new FakeProvider(expected);
        var orchestrator = new BookLookupOrchestrator(
            [provider],
            NullLogger<BookLookupOrchestrator>.Instance);
        var service = new IsbnMetadataService(orchestrator);

        var result = await service.ResolveIsbnAsync("9780306406157");

        Assert.NotNull(result);
        Assert.Equal("Test Book", result.Title);
        Assert.Equal("TestProvider", result.Source);
    }

    [Fact]
    public async Task ResolveIsbnAsync_NoProviderResult_ReturnsNull()
    {
        var provider = new FakeProvider(null);
        var orchestrator = new BookLookupOrchestrator(
            [provider],
            NullLogger<BookLookupOrchestrator>.Instance);
        var service = new IsbnMetadataService(orchestrator);

        var result = await service.ResolveIsbnAsync("9780306406157");

        Assert.Null(result);
    }

    [Fact]
    public async Task ResolveIsbnAsync_MultipleProviders_FallsBack()
    {
        var fallback = new BookMetadata { Title = "Fallback Book", Source = "Secondary" };
        var primary = new FakeProvider(null);
        var secondary = new FakeProvider(fallback);
        var orchestrator = new BookLookupOrchestrator(
            [primary, secondary],
            NullLogger<BookLookupOrchestrator>.Instance);
        var service = new IsbnMetadataService(orchestrator);

        var result = await service.ResolveIsbnAsync("9780306406157");

        Assert.NotNull(result);
        Assert.Equal("Fallback Book", result.Title);
    }

    private sealed class FakeProvider(BookMetadata? result) : IBookMetadataProvider
    {
        public string SourceName => "Fake";
        public Task<BookMetadata?> LookupAsync(string isbn, CancellationToken cancellationToken = default)
            => Task.FromResult(result);
    }
}
