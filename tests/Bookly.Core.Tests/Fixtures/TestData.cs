using Bookly.Core.Entities;
using Bookly.Core.Models;

namespace Bookly.Core.Tests.Fixtures;

public static class TestData
{
    public const string ValidIsbn13 = "9780306406157";
    public const string ValidIsbn13WithDashes = "978-0-306-40615-7";
    public const string ValidIsbn10 = "0306406152";
    public const string InvalidIsbn = "not-an-isbn";
    public const string AnotherValidIsbn13 = "9783161484100";

    public static Book SampleBook(string? normalizedIsbn = null) => new()
    {
        Title = "Sample Book",
        NormalizedIsbn = normalizedIsbn ?? ValidIsbn13,
        MetadataSource = "test",
        CreatedAtUtc = DateTime.UtcNow,
        UpdatedAtUtc = DateTime.UtcNow,
    };

    public static Author SampleAuthor(string name = "Test Author") => new()
    {
        Name = name,
    };

    public static BookMetadata SampleBookMetadata() => new()
    {
        Isbn13 = ValidIsbn13,
        Title = "Sample Metadata Book",
        Authors = ["Author One", "Author Two"],
        Publisher = "Test Publisher",
        Source = "test",
    };
}
