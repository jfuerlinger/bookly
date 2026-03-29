using Bookly.Core.Models;
using Bookly.Core.Services;

namespace Bookly.Api.Integration.Tests.Infrastructure;

/// <summary>
/// Deterministic metadata provider that returns known data for test ISBNs
/// without making external HTTP calls.
/// </summary>
internal sealed class StubMetadataProvider : IBookMetadataProvider
{
    public string SourceName => "Stub";

    public Task<BookMetadata?> LookupAsync(string isbn, CancellationToken cancellationToken = default)
    {
        // Return predictable metadata for any ISBN
        var metadata = new BookMetadata
        {
            Isbn10 = isbn.Length == 10 ? isbn : null,
            Isbn13 = isbn.Length == 13 ? isbn : null,
            Title = $"Test Book for {isbn}",
            Subtitle = "A Stub Subtitle",
            Authors = ["Test Author"],
            Publisher = "Test Publisher",
            PublishedOn = new DateOnly(2024, 1, 15),
            Language = "en",
            PageCount = 320,
            Description = "A book created by the test stub provider.",
            CoverSmallUrl = null,
            CoverMediumUrl = null,
            CoverLargeUrl = null,
            Source = SourceName,
        };

        return Task.FromResult<BookMetadata?>(metadata);
    }
}
