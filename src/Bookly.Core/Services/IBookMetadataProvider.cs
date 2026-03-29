using Bookly.Core.Models;

namespace Bookly.Core.Services;

public interface IBookMetadataProvider
{
    string SourceName { get; }
    Task<BookMetadata?> LookupAsync(string isbn, CancellationToken cancellationToken = default);
}
