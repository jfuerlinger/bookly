using Bookly.Core.Models;

namespace Bookly.Core.UseCases;

public interface IListBooksUseCase
{
    Task<IEnumerable<BookDto>> ExecuteAsync(
        int skip = 0,
        int take = 20,
        CancellationToken cancellationToken = default);
}
