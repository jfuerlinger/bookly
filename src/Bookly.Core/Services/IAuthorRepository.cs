using Bookly.Core.Entities;

namespace Bookly.Core.Services;

public interface IAuthorRepository
{
    Task<Author?> GetAuthorByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Author?> GetAuthorByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Author>> GetAllAuthorsAsync(int skip = 0, int take = 10, CancellationToken cancellationToken = default);
    Task<Author> CreateAuthorAsync(Author author, CancellationToken cancellationToken = default);
    Task<Author> UpdateAuthorAsync(Author author, CancellationToken cancellationToken = default);
    Task<bool> DeleteAuthorAsync(int id, CancellationToken cancellationToken = default);
}
