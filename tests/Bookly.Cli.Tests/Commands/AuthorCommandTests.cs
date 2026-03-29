using Bookly.Cli.Commands.Authors;
using Bookly.Core.Data;
using Bookly.Core.Entities;
using Bookly.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace Bookly.Cli.Tests.Commands;

public class AuthorCommandTests : IAsyncLifetime
{
    private BooklyDbContext _dbContext = null!;
    private AuthorRepository _repository = null!;

    public async Task InitializeAsync()
    {
        var opts = new DbContextOptionsBuilder<BooklyDbContext>()
            .UseInMemoryDatabase($"author-test-{Guid.NewGuid()}")
            .Options;
        _dbContext = new BooklyDbContext(opts);
        await _dbContext.Database.EnsureCreatedAsync();
        _repository = new AuthorRepository(_dbContext);
    }

    [Fact]
    public async Task AddAuthor_ValidName_ExitCode0()
    {
        var command = new AddAuthorCommand(_repository, _dbContext);
        var exitCode = await command.ExecuteAsync(["Test Author"], CancellationToken.None);
        Assert.Equal(0, exitCode);
        var author = await _repository.GetAuthorByNameAsync("Test Author");
        Assert.NotNull(author);
    }

    [Fact]
    public async Task AddAuthor_EmptyName_ExitCode1()
    {
        var command = new AddAuthorCommand(_repository, _dbContext);
        var exitCode = await command.ExecuteAsync([""], CancellationToken.None);
        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task AddAuthor_DuplicateName_ExitCode1()
    {
        await _repository.CreateAuthorAsync(new Author { Name = "Existing Author" });
        await _dbContext.SaveChangesAsync();

        var command = new AddAuthorCommand(_repository, _dbContext);
        var exitCode = await command.ExecuteAsync(["Existing Author"], CancellationToken.None);
        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task ListAuthors_ExitCode0()
    {
        var command = new ListAuthorsCommand(_repository);
        var exitCode = await command.ExecuteAsync([], CancellationToken.None);
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task GetAuthor_ExistingId_ExitCode0()
    {
        var author = await _repository.CreateAuthorAsync(new Author { Name = "Get Author" });
        await _dbContext.SaveChangesAsync();

        var command = new GetAuthorCommand(_repository);
        var exitCode = await command.ExecuteAsync([author.Id.ToString()], CancellationToken.None);
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task GetAuthor_NonExistentId_ExitCode1()
    {
        var command = new GetAuthorCommand(_repository);
        var exitCode = await command.ExecuteAsync(["99999"], CancellationToken.None);
        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task DeleteAuthor_ExistingId_WithForce_ExitCode0()
    {
        var author = await _repository.CreateAuthorAsync(new Author { Name = "Delete Author" });
        await _dbContext.SaveChangesAsync();

        var command = new DeleteAuthorCommand(_repository, _dbContext);
        var exitCode = await command.ExecuteAsync([author.Id.ToString(), "--force"], CancellationToken.None);
        Assert.Equal(0, exitCode);
        var deleted = await _repository.GetAuthorByIdAsync(author.Id);
        Assert.Null(deleted);
    }

    public async Task DisposeAsync()
    {
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.DisposeAsync();
    }
}
