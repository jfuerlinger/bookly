using System.Globalization;
using System.Text.Json;
using Bookly.Core.Models;
using Microsoft.Extensions.Logging;

namespace Bookly.Core.Services;

public sealed class OpenLibraryProvider(
    HttpClient httpClient,
    ILogger<OpenLibraryProvider> logger) : IBookMetadataProvider
{
    public string SourceName => "OpenLibrary";

    public async Task<BookMetadata?> LookupAsync(string isbn, CancellationToken cancellationToken = default)
    {
        var url = $"https://openlibrary.org/api/books?bibkeys=ISBN:{isbn}&format=json&jscmd=data";
        logger.LogInformation("OpenLibrary lookup for ISBN {Isbn}", isbn);

        var response = await httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);

        var key = $"ISBN:{isbn}";
        if (!doc.RootElement.TryGetProperty(key, out var book))
        {
            logger.LogInformation("OpenLibrary: no result for ISBN {Isbn}", isbn);
            return null;
        }

        var title = book.GetPropertyOrDefault("title");
        if (title is null)
            return null;

        var authors = new List<string>();
        if (book.TryGetProperty("authors", out var authorsEl))
        {
            foreach (var a in authorsEl.EnumerateArray())
            {
                var name = a.GetPropertyOrDefault("name");
                if (name is not null)
                    authors.Add(name);
            }
        }

        DateOnly? publishedOn = null;
        var publishDate = book.GetPropertyOrDefault("publish_date");
        if (publishDate is not null)
            publishedOn = TryParseDate(publishDate);

        string? coverSmall = null, coverMedium = null, coverLarge = null;
        if (book.TryGetProperty("cover", out var cover))
        {
            coverSmall = cover.GetPropertyOrDefault("small");
            coverMedium = cover.GetPropertyOrDefault("medium");
            coverLarge = cover.GetPropertyOrDefault("large");
        }

        // Try to extract ISBN-10 and ISBN-13 from identifiers
        string? isbn10 = null, isbn13 = null;
        if (book.TryGetProperty("identifiers", out var ids))
        {
            isbn10 = GetFirstIdentifier(ids, "isbn_10");
            isbn13 = GetFirstIdentifier(ids, "isbn_13");
        }
        // Fallback: assign based on length
        isbn10 ??= isbn.Length == 10 ? isbn : null;
        isbn13 ??= isbn.Length == 13 ? isbn : null;

        return new BookMetadata
        {
            Isbn10 = isbn10,
            Isbn13 = isbn13,
            Title = title,
            Subtitle = book.GetPropertyOrDefault("subtitle"),
            Authors = authors,
            Publisher = GetFirstPublisher(book),
            PublishedOn = publishedOn,
            Language = null, // OpenLibrary data endpoint doesn't reliably return language
            PageCount = book.TryGetProperty("number_of_pages", out var pages) && pages.TryGetInt32(out var pc) ? pc : null,
            Description = ExtractDescription(book),
            CoverSmallUrl = coverSmall,
            CoverMediumUrl = coverMedium,
            CoverLargeUrl = coverLarge,
            Source = SourceName,
        };
    }

    private static string? GetFirstPublisher(JsonElement book)
    {
        if (!book.TryGetProperty("publishers", out var publishers))
            return null;

        foreach (var p in publishers.EnumerateArray())
        {
            var name = p.GetPropertyOrDefault("name");
            if (name is not null)
                return name;
        }
        return null;
    }

    private static string? GetFirstIdentifier(JsonElement ids, string key)
    {
        if (!ids.TryGetProperty(key, out var arr))
            return null;

        foreach (var v in arr.EnumerateArray())
        {
            var s = v.GetString();
            if (!string.IsNullOrWhiteSpace(s))
                return s;
        }
        return null;
    }

    private static string? ExtractDescription(JsonElement book)
    {
        if (!book.TryGetProperty("excerpts", out var excerpts))
            return null;

        foreach (var e in excerpts.EnumerateArray())
        {
            var text = e.GetPropertyOrDefault("text");
            if (text is not null)
                return text;
        }
        return null;
    }

    private static DateOnly? TryParseDate(string value)
    {
        string[] formats = ["MMMM d, yyyy", "MMMM yyyy", "yyyy", "yyyy-MM-dd", "MMM d, yyyy"];
        foreach (var fmt in formats)
        {
            if (DateOnly.TryParseExact(value, fmt, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                return date;
        }
        return null;
    }
}

internal static class JsonElementExtensions
{
    public static string? GetPropertyOrDefault(this JsonElement element, string name) =>
        element.TryGetProperty(name, out var prop) && prop.ValueKind == JsonValueKind.String
            ? prop.GetString()
            : null;
}
