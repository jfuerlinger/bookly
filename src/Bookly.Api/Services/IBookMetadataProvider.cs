using Bookly.Api.Models;

namespace Bookly.Api.Services;

public interface IBookMetadataProvider
{
    string SourceName { get; }
    Task<BookMetadata?> LookupAsync(string isbn, CancellationToken cancellationToken = default);
}
