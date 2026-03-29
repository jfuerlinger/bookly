using System.Globalization;
using System.Text.Json;
using Bookly.Core.Models;
using Microsoft.Extensions.Logging;

namespace Bookly.Core.Services;

public sealed class GoogleBooksProvider(
    HttpClient httpClient,
    ILogger<GoogleBooksProvider> logger) : IBookMetadataProvider
{
    public string SourceName => "GoogleBooks";

    public async Task<BookMetadata?> LookupAsync(string isbn, CancellationToken cancellationToken = default)
    {
        var url = $"https://www.googleapis.com/books/v1/volumes?q=isbn:{isbn}";
        logger.LogInformation("GoogleBooks lookup for ISBN {Isbn}", isbn);

        var response = await httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("totalItems", out var total) || total.GetInt32() == 0)
        {
            logger.LogInformation("GoogleBooks: no result for ISBN {Isbn}", isbn);
            return null;
        }

        if (!doc.RootElement.TryGetProperty("items", out var items))
            return null;

        var first = items.EnumerateArray().FirstOrDefault();
        if (first.ValueKind == JsonValueKind.Undefined)
            return null;

        if (!first.TryGetProperty("volumeInfo", out var vol))
            return null;

        var title = vol.GetPropertyOrDefault("title");
        if (title is null)
            return null;

        var authors = new List<string>();
        if (vol.TryGetProperty("authors", out var authorsEl))
        {
            foreach (var a in authorsEl.EnumerateArray())
            {
                var name = a.GetString();
                if (!string.IsNullOrWhiteSpace(name))
                    authors.Add(name);
            }
        }

        DateOnly? publishedOn = null;
        var pubDate = vol.GetPropertyOrDefault("publishedDate");
        if (pubDate is not null)
            publishedOn = TryParseDate(pubDate);

        string? coverSmall = null, coverMedium = null, coverLarge = null;
        if (vol.TryGetProperty("imageLinks", out var images))
        {
            coverSmall = images.GetPropertyOrDefault("smallThumbnail");
            coverMedium = images.GetPropertyOrDefault("thumbnail");
            coverLarge = images.GetPropertyOrDefault("large")
                         ?? images.GetPropertyOrDefault("extraLarge");
        }

        // Extract ISBNs from industry identifiers
        string? isbn10 = null, isbn13 = null;
        if (vol.TryGetProperty("industryIdentifiers", out var identifiers))
        {
            foreach (var id in identifiers.EnumerateArray())
            {
                var type = id.GetPropertyOrDefault("type");
                var identifier = id.GetPropertyOrDefault("identifier");
                if (type == "ISBN_10") isbn10 = identifier;
                if (type == "ISBN_13") isbn13 = identifier;
            }
        }
        isbn10 ??= isbn.Length == 10 ? isbn : null;
        isbn13 ??= isbn.Length == 13 ? isbn : null;

        return new BookMetadata
        {
            Isbn10 = isbn10,
            Isbn13 = isbn13,
            Title = title,
            Subtitle = vol.GetPropertyOrDefault("subtitle"),
            Authors = authors,
            Publisher = vol.GetPropertyOrDefault("publisher"),
            PublishedOn = publishedOn,
            Language = vol.GetPropertyOrDefault("language"),
            PageCount = vol.TryGetProperty("pageCount", out var pages) && pages.TryGetInt32(out var pc) && pc > 0 ? pc : null,
            Description = vol.GetPropertyOrDefault("description"),
            CoverSmallUrl = coverSmall,
            CoverMediumUrl = coverMedium,
            CoverLargeUrl = coverLarge,
            Source = SourceName,
        };
    }

    private static DateOnly? TryParseDate(string value)
    {
        string[] formats = ["yyyy-MM-dd", "yyyy-MM", "yyyy"];
        foreach (var fmt in formats)
        {
            if (DateOnly.TryParseExact(value, fmt, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                return date;
        }
        return null;
    }
}
