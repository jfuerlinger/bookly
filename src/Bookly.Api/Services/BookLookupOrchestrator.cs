using Bookly.Api.Models;

namespace Bookly.Api.Services;

public sealed class BookLookupOrchestrator(
    IEnumerable<IBookMetadataProvider> providers,
    ILogger<BookLookupOrchestrator> logger)
{
    public async Task<BookMetadata?> LookupAsync(string isbn, CancellationToken cancellationToken = default)
    {
        foreach (var provider in providers)
        {
            try
            {
                logger.LogInformation("Trying metadata provider {Provider} for ISBN {Isbn}",
                    provider.SourceName, isbn);

                var result = await provider.LookupAsync(isbn, cancellationToken);
                if (result is not null)
                {
                    logger.LogInformation("Provider {Provider} returned metadata for ISBN {Isbn}",
                        provider.SourceName, isbn);
                    return result;
                }

                logger.LogInformation("Provider {Provider} returned no result for ISBN {Isbn}, falling back",
                    provider.SourceName, isbn);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(ex,
                    "Provider {Provider} failed for ISBN {Isbn}, falling back",
                    provider.SourceName, isbn);
            }
        }

        logger.LogWarning("All providers failed for ISBN {Isbn}", isbn);
        return null;
    }
}
