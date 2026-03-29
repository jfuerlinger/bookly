namespace Bookly.Core.Entities;

public class Book
{
    public int Id { get; set; }

    public string? Isbn10 { get; set; }
    public string? Isbn13 { get; set; }
    public required string NormalizedIsbn { get; set; }

    public required string Title { get; set; }
    public string? Subtitle { get; set; }
    public string? Publisher { get; set; }
    public DateOnly? PublishedOn { get; set; }
    public string? Language { get; set; }
    public int? PageCount { get; set; }
    public string? Description { get; set; }

    public string? CoverSmallUrl { get; set; }
    public string? CoverMediumUrl { get; set; }
    public string? CoverLargeUrl { get; set; }

    public required string MetadataSource { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public ICollection<BookAuthor> BookAuthors { get; set; } = [];
}
