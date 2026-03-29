using System.Text.Json;
using Bookly.Core.Isbn;
using Bookly.Core.Models;

namespace Bookly.Core.Services;

public sealed class IsbnMetadataService : IIsbnMetadataService
{
    private readonly HttpClient _httpClient;
    private readonly bool _enableFallback;

    public IsbnMetadataService(HttpClient httpClient, bool enableFallback = false)
    {
        _httpClient = httpClient;
        _enableFallback = enableFallback;
    }

    public async Task<BookMetadata?> ResolveIsbnAsync(string isbn, CancellationToken cancellationToken = default)
    {
        var validation = IsbnValidator.Validate(isbn);
        if (!validation.IsValid)
            return null;

        var normalizedIsbn = validation.NormalizedIsbn!;

        try
        {
            var url = $"https://openlibrary.org/api/books?bibkeys=ISBN:{normalizedIsbn}&format=json&jscmd=data";
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return _enableFallback ? CreateFallback(normalizedIsbn, validation) : null;

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return ParseOpenLibraryResponse(content, normalizedIsbn, validation)
                ?? (_enableFallback ? CreateFallback(normalizedIsbn, validation) : null);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _ = ex;
            return _enableFallback ? CreateFallback(normalizedIsbn, validation) : null;
        }
    }

    private static BookMetadata? ParseOpenLibraryResponse(string content, string normalizedIsbn, IsbnValidationResult validation)
    {
        try
        {
            using var doc = JsonDocument.Parse(content);
            var key = $"ISBN:{normalizedIsbn}";
            if (!doc.RootElement.TryGetProperty(key, out var bookElement))
                return null;

            var title = bookElement.TryGetProperty("title", out var titleProp) ? titleProp.GetString() : null;
            if (string.IsNullOrWhiteSpace(title))
                return null;

            var authors = new List<string>();
            if (bookElement.TryGetProperty("authors", out var authorsProp))
            {
                foreach (var author in authorsProp.EnumerateArray())
                {
                    if (author.TryGetProperty("name", out var nameProp))
                    {
                        var name = nameProp.GetString();
                        if (!string.IsNullOrWhiteSpace(name))
                            authors.Add(name);
                    }
                }
            }

            string? publisher = null;
            if (bookElement.TryGetProperty("publishers", out var publishersProp) && publishersProp.GetArrayLength() > 0)
                publisher = publishersProp[0].TryGetProperty("name", out var pName) ? pName.GetString() : null;

            DateOnly? publishedOn = null;
            if (bookElement.TryGetProperty("publish_date", out var dateProp))
            {
                var dateStr = dateProp.GetString();
                if (!string.IsNullOrWhiteSpace(dateStr))
                {
                    if (DateOnly.TryParse(dateStr, out var d))
                        publishedOn = d;
                    else if (int.TryParse(dateStr, out var year))
                        publishedOn = new DateOnly(year, 1, 1);
                }
            }

            string? coverSmall = null, coverMedium = null, coverLarge = null;
            if (bookElement.TryGetProperty("cover", out var coverProp))
            {
                coverSmall = coverProp.TryGetProperty("small", out var s) ? s.GetString() : null;
                coverMedium = coverProp.TryGetProperty("medium", out var m) ? m.GetString() : null;
                coverLarge = coverProp.TryGetProperty("large", out var l) ? l.GetString() : null;
            }

            return new BookMetadata
            {
                Isbn10 = validation.Isbn10,
                Isbn13 = validation.Isbn13 ?? normalizedIsbn,
                Title = title,
                Authors = authors,
                Publisher = publisher,
                PublishedOn = publishedOn,
                CoverSmallUrl = coverSmall,
                CoverMediumUrl = coverMedium,
                CoverLargeUrl = coverLarge,
                Source = "openlibrary"
            };
        }
        catch
        {
            return null;
        }
    }

    private static BookMetadata CreateFallback(string normalizedIsbn, IsbnValidationResult validation) =>
        new()
        {
            Isbn10 = validation.Isbn10,
            Isbn13 = validation.Isbn13 ?? normalizedIsbn,
            Title = "[Fallback] Unknown Book",
            Authors = [],
            Source = "fallback"
        };
}
