using Bookly.Core.Entities;

namespace Bookly.Core.Services;

public interface IBookRepository
{
    Task<Book?> GetBookByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Book?> GetBookByIsbnAsync(string normalizedIsbn, CancellationToken cancellationToken = default);
    Task<IEnumerable<Book>> GetAllBooksAsync(int skip = 0, int take = 10, CancellationToken cancellationToken = default);
    Task<Book> CreateBookAsync(Book book, CancellationToken cancellationToken = default);
    Task<Book> UpdateBookAsync(Book book, CancellationToken cancellationToken = default);
    Task<bool> DeleteBookAsync(int id, CancellationToken cancellationToken = default);
}
