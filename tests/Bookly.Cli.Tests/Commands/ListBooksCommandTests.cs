using Bookly.Cli.Commands.Books;
using Bookly.Core.Data;
using Bookly.Core.Entities;
using Bookly.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace Bookly.Cli.Tests.Commands;

public class ListBooksCommandTests : IAsyncLifetime
{
    private BooklyDbContext _dbContext = null!;
    private BookRepository _repository = null!;
    private ListBooksCommand _command = null!;

    public async Task InitializeAsync()
    {
        var opts = new DbContextOptionsBuilder<BooklyDbContext>()
            .UseInMemoryDatabase($"list-test-{Guid.NewGuid()}")
            .Options;
        _dbContext = new BooklyDbContext(opts);
        await _dbContext.Database.EnsureCreatedAsync();
        _repository = new BookRepository(_dbContext);
        _command = new ListBooksCommand(_repository);
    }

    [Fact]
    public async Task ListBooks_EmptyDb_ExitCode0()
    {
        var exitCode = await _command.ExecuteAsync([], CancellationToken.None);
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task ListBooks_WithBooks_ExitCode0()
    {
        await _repository.CreateBookAsync(new Book
        {
            Title = "Test Book",
            NormalizedIsbn = "9780306406157",
            MetadataSource = "test",
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        });
        await _dbContext.SaveChangesAsync();

        var exitCode = await _command.ExecuteAsync([], CancellationToken.None);
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task ListBooks_JsonFormat_ExitCode0()
    {
        var exitCode = await _command.ExecuteAsync(["--output", "json"], CancellationToken.None);
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task ListBooks_PaginationArgs_ExitCode0()
    {
        var exitCode = await _command.ExecuteAsync(["--skip", "0", "--take", "5"], CancellationToken.None);
        Assert.Equal(0, exitCode);
    }

    public async Task DisposeAsync()
    {
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.DisposeAsync();
    }
}
