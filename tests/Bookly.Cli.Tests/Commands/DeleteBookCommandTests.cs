using Bookly.Cli.Commands.Books;
using Bookly.Core.Data;
using Bookly.Core.Entities;
using Bookly.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace Bookly.Cli.Tests.Commands;

public class DeleteBookCommandTests : IAsyncLifetime
{
    private BooklyDbContext _dbContext = null!;
    private BookRepository _repository = null!;
    private DeleteBookCommand _command = null!;

    public async Task InitializeAsync()
    {
        var opts = new DbContextOptionsBuilder<BooklyDbContext>()
            .UseInMemoryDatabase($"delete-test-{Guid.NewGuid()}")
            .Options;
        _dbContext = new BooklyDbContext(opts);
        await _dbContext.Database.EnsureCreatedAsync();
        _repository = new BookRepository(_dbContext);
        _command = new DeleteBookCommand(_repository, _dbContext);
    }

    [Fact]
    public async Task DeleteBook_ExistingBook_WithForce_ExitCode0()
    {
        var book = new Book
        {
            Title = "To Delete",
            NormalizedIsbn = "9780306406157",
            MetadataSource = "test",
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        var created = await _repository.CreateBookAsync(book);
        await _dbContext.SaveChangesAsync();

        var exitCode = await _command.ExecuteAsync([created.Id.ToString(), "--force"], CancellationToken.None);

        Assert.Equal(0, exitCode);
        var deleted = await _repository.GetBookByIdAsync(created.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteBook_NonExistentId_ExitCode1()
    {
        var exitCode = await _command.ExecuteAsync(["99999", "--force"], CancellationToken.None);
        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task DeleteBook_InvalidId_ExitCode1()
    {
        var exitCode = await _command.ExecuteAsync(["not-a-number", "--force"], CancellationToken.None);
        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task DeleteBook_NoArgs_ExitCode1()
    {
        var exitCode = await _command.ExecuteAsync([], CancellationToken.None);
        Assert.Equal(1, exitCode);
    }

    public async Task DisposeAsync()
    {
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.DisposeAsync();
    }
}
