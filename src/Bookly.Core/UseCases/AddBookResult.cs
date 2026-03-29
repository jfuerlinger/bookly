using Bookly.Core.Entities;

namespace Bookly.Core.UseCases;

public enum AddBookOutcome
{
    Created,
    AlreadyExists,
    ValidationFailed,
    MetadataNotFound
}

public sealed record AddBookResult
{
    public Book? Book { get; init; }
    public required AddBookOutcome Outcome { get; init; }
    public string? Error { get; init; }
}
