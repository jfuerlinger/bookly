using Bookly.Core.Data;
using Bookly.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bookly.Core.Services;

public sealed class BookRepository : IBookRepository
{
    private readonly BooklyDbContext _dbContext;

    public BookRepository(BooklyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Book?> GetBookByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await _dbContext.Books
            .Include(b => b.BookAuthors)
            .ThenInclude(ba => ba.Author)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public async Task<Book?> GetBookByIsbnAsync(string normalizedIsbn, CancellationToken cancellationToken = default) =>
        await _dbContext.Books
            .Include(b => b.BookAuthors)
            .ThenInclude(ba => ba.Author)
            .FirstOrDefaultAsync(b => b.NormalizedIsbn == normalizedIsbn, cancellationToken);

    public async Task<IEnumerable<Book>> GetAllBooksAsync(int skip = 0, int take = 10, CancellationToken cancellationToken = default) =>
        await _dbContext.Books
            .Include(b => b.BookAuthors)
            .ThenInclude(ba => ba.Author)
            .OrderBy(b => b.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public Task<Book> CreateBookAsync(Book book, CancellationToken cancellationToken = default)
    {
        _dbContext.Books.Add(book);
        return Task.FromResult(book);
    }

    public Task<Book> UpdateBookAsync(Book book, CancellationToken cancellationToken = default)
    {
        book.UpdatedAtUtc = DateTime.UtcNow;
        _dbContext.Books.Update(book);
        return Task.FromResult(book);
    }

    public async Task<bool> DeleteBookAsync(int id, CancellationToken cancellationToken = default)
    {
        var book = await _dbContext.Books.FindAsync([id], cancellationToken);
        if (book is null)
            return false;

        _dbContext.Books.Remove(book);
        return true;
    }
}
