using Bookly.Core.Data;
using Bookly.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bookly.Core.Services;

public sealed class AuthorRepository : IAuthorRepository
{
    private readonly BooklyDbContext _dbContext;

    public AuthorRepository(BooklyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Author?> GetAuthorByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await _dbContext.Authors
            .Include(a => a.BookAuthors)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<Author?> GetAuthorByNameAsync(string name, CancellationToken cancellationToken = default) =>
        await _dbContext.Authors
            .Include(a => a.BookAuthors)
            .FirstOrDefaultAsync(a => a.Name == name, cancellationToken);

    public async Task<IEnumerable<Author>> GetAllAuthorsAsync(int skip = 0, int take = 10, CancellationToken cancellationToken = default) =>
        await _dbContext.Authors
            .Include(a => a.BookAuthors)
            .OrderBy(a => a.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public Task<Author> CreateAuthorAsync(Author author, CancellationToken cancellationToken = default)
    {
        _dbContext.Authors.Add(author);
        return Task.FromResult(author);
    }

    public Task<Author> UpdateAuthorAsync(Author author, CancellationToken cancellationToken = default)
    {
        _dbContext.Authors.Update(author);
        return Task.FromResult(author);
    }

    public async Task<bool> DeleteAuthorAsync(int id, CancellationToken cancellationToken = default)
    {
        var author = await _dbContext.Authors.FindAsync([id], cancellationToken);
        if (author is null)
            return false;

        _dbContext.Authors.Remove(author);
        return true;
    }
}
