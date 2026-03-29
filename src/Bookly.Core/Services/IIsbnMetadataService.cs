using Bookly.Core.Models;

namespace Bookly.Core.Services;

public interface IIsbnMetadataService
{
    Task<BookMetadata?> ResolveIsbnAsync(string isbn, CancellationToken cancellationToken = default);
}
