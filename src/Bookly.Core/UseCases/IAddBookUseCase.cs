using Bookly.Core.Entities;

namespace Bookly.Core.UseCases;

public interface IAddBookUseCase
{
    Task<Book> ExecuteAsync(
        string isbn,
        string? titleOverride = null,
        IEnumerable<string>? authorOverrides = null,
        CancellationToken cancellationToken = default);
}
