namespace Bookly.Api.Models;

public sealed record IsbnScanRequest
{
    public string? Isbn { get; init; }
}
