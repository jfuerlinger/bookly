using Bookly.Core.Models;

namespace Bookly.Core.Services;

public sealed class IsbnMetadataService(BookLookupOrchestrator orchestrator) : IIsbnMetadataService
{
    public Task<BookMetadata?> ResolveIsbnAsync(string isbn, CancellationToken cancellationToken = default)
        => orchestrator.LookupAsync(isbn, cancellationToken);
}
