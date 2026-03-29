using Bookly.Api.Models;
using Bookly.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bookly.Api.Tests;

public class BookLookupOrchestratorTests
{
    [Fact]
    public async Task LookupAsync_PrimarySucceeds_ReturnsPrimaryResult()
    {
        var primary = new FakeProvider("Primary", new BookMetadata
        {
            Title = "From Primary",
            Source = "Primary"
        });
        var secondary = new FakeProvider("Secondary", new BookMetadata
        {
            Title = "From Secondary",
            Source = "Secondary"
        });

        var orchestrator = new BookLookupOrchestrator(
            [primary, secondary],
            NullLogger<BookLookupOrchestrator>.Instance);

        var result = await orchestrator.LookupAsync("1234567890");

        Assert.NotNull(result);
        Assert.Equal("From Primary", result.Title);
        Assert.False(secondary.WasCalled);
    }

    [Fact]
    public async Task LookupAsync_PrimaryReturnsNull_FallsBackToSecondary()
    {
        var primary = new FakeProvider("Primary", null);
        var secondary = new FakeProvider("Secondary", new BookMetadata
        {
            Title = "From Secondary",
            Source = "Secondary"
        });

        var orchestrator = new BookLookupOrchestrator(
            [primary, secondary],
            NullLogger<BookLookupOrchestrator>.Instance);

        var result = await orchestrator.LookupAsync("1234567890");

        Assert.NotNull(result);
        Assert.Equal("From Secondary", result.Title);
        Assert.True(primary.WasCalled);
        Assert.True(secondary.WasCalled);
    }

    [Fact]
    public async Task LookupAsync_PrimaryThrows_FallsBackToSecondary()
    {
        var primary = new ThrowingProvider("Primary");
        var secondary = new FakeProvider("Secondary", new BookMetadata
        {
            Title = "Fallback",
            Source = "Secondary"
        });

        var orchestrator = new BookLookupOrchestrator(
            [primary, secondary],
            NullLogger<BookLookupOrchestrator>.Instance);

        var result = await orchestrator.LookupAsync("1234567890");

        Assert.NotNull(result);
        Assert.Equal("Fallback", result.Title);
    }

    [Fact]
    public async Task LookupAsync_AllProvidersFail_ReturnsNull()
    {
        var primary = new ThrowingProvider("Primary");
        var secondary = new FakeProvider("Secondary", null);

        var orchestrator = new BookLookupOrchestrator(
            [primary, secondary],
            NullLogger<BookLookupOrchestrator>.Instance);

        var result = await orchestrator.LookupAsync("1234567890");

        Assert.Null(result);
    }

    private sealed class FakeProvider(string name, BookMetadata? result) : IBookMetadataProvider
    {
        public string SourceName => name;
        public bool WasCalled { get; private set; }

        public Task<BookMetadata?> LookupAsync(string isbn, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.FromResult(result);
        }
    }

    private sealed class ThrowingProvider(string name) : IBookMetadataProvider
    {
        public string SourceName => name;
        public Task<BookMetadata?> LookupAsync(string isbn, CancellationToken cancellationToken = default)
            => throw new HttpRequestException("Simulated failure");
    }
}
