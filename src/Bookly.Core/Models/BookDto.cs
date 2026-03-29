namespace Bookly.Core.Models;

public sealed record BookDto
{
    public int Id { get; init; }
    public string? Isbn10 { get; init; }
    public string? Isbn13 { get; init; }
    public required string NormalizedIsbn { get; init; }
    public required string Title { get; init; }
    public string? Subtitle { get; init; }
    public List<string> Authors { get; init; } = [];
    public string? Publisher { get; init; }
    public DateOnly? PublishedOn { get; init; }
    public string? Language { get; init; }
    public int? PageCount { get; init; }
    public string? Description { get; init; }
    public string? CoverSmallUrl { get; init; }
    public string? CoverMediumUrl { get; init; }
    public string? CoverLargeUrl { get; init; }
    public required string MetadataSource { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime UpdatedAtUtc { get; init; }
}
