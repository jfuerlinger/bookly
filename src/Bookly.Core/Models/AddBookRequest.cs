namespace Bookly.Core.Models;

public sealed record AddBookRequest
{
    public required string Isbn { get; init; }
    public string? ManualTitle { get; init; }
    public List<string> ManualAuthors { get; init; } = [];
}
