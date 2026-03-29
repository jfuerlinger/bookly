using Bookly.Cli.Commands.Books;
using Bookly.Core.Data;
using Bookly.Core.Models;
using Bookly.Core.Services;
using Bookly.Core.UseCases;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bookly.Cli.Tests.Commands;

public class AddBookCommandTests_InMemory : IAsyncLifetime
{
    private BooklyDbContext _dbContext = null!;
    private BookRepository _bookRepository = null!;
    private AddBookByIsbnCommand _command = null!;

    public async Task InitializeAsync()
    {
        var opts = new DbContextOptionsBuilder<BooklyDbContext>()
            .UseInMemoryDatabase($"cli-add-test-{Guid.NewGuid()}")
            .Options;
        _dbContext = new BooklyDbContext(opts);
        await _dbContext.Database.EnsureCreatedAsync();
        _bookRepository = new BookRepository(_dbContext);
        var authorRepository = new AuthorRepository(_dbContext);

        // Provider that returns fallback metadata for any ISBN
        var provider = new FakeProvider(new BookMetadata
        {
            Title = "[Fallback] Unknown Book",
            Authors = [],
            Source = "fallback"
        });
        var orchestrator = new BookLookupOrchestrator(
            [provider],
            NullLogger<BookLookupOrchestrator>.Instance);
        var metadataService = new IsbnMetadataService(orchestrator);
        var useCase = new AddBookUseCase(_bookRepository, authorRepository, metadataService, _dbContext);
        _command = new AddBookByIsbnCommand(useCase);
    }

    [Fact]
    public async Task AddBookCommand_ValidIsbn_ExitCode0()
    {
        var exitCode = await _command.ExecuteAsync(["9780306406157"], CancellationToken.None);
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task AddBookCommand_ValidIsbn_CreatesBookInDb()
    {
        await _command.ExecuteAsync(["9780306406157"], CancellationToken.None);
        var book = await _bookRepository.GetBookByIsbnAsync("9780306406157");
        Assert.NotNull(book);
    }

    [Fact]
    public async Task AddBookCommand_InvalidIsbn_ExitCode1()
    {
        var exitCode = await _command.ExecuteAsync(["not-an-isbn"], CancellationToken.None);
        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task AddBookCommand_WithTitleOverride_UsesTitleOverride()
    {
        await _command.ExecuteAsync(["9780306406157", "--title", "Custom Title"], CancellationToken.None);
        var book = await _bookRepository.GetBookByIsbnAsync("9780306406157");
        Assert.Equal("Custom Title", book?.Title);
    }

    [Fact]
    public async Task AddBookCommand_NoArgs_ExitCode1()
    {
        var exitCode = await _command.ExecuteAsync([], CancellationToken.None);
        Assert.Equal(1, exitCode);
    }

    public async Task DisposeAsync()
    {
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.DisposeAsync();
    }

    private sealed class FakeProvider(BookMetadata? result) : IBookMetadataProvider
    {
        public string SourceName => "Fake";
        public Task<BookMetadata?> LookupAsync(string isbn, CancellationToken cancellationToken = default)
            => Task.FromResult(result);
    }
}

[Collection(nameof(Fixtures.PostgresqlFixtureCollection))]
public class AddBookCommandTests_Postgres : IAsyncLifetime
{
    private readonly Fixtures.PostgresqlFixture _fixture;
    private BooklyDbContext _dbContext = null!;

    public AddBookCommandTests_Postgres(Fixtures.PostgresqlFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        await _fixture.CleanAsync();
        _dbContext = _fixture.CreateDbContext();
    }

    [Fact]
    public async Task AddBookCommand_WithRealDb_CreatesBook()
    {
        var bookRepository = new BookRepository(_dbContext);
        var authorRepository = new AuthorRepository(_dbContext);
        var provider = new FakeProvider(new BookMetadata
        {
            Title = "[Fallback] Unknown Book",
            Authors = [],
            Source = "fallback"
        });
        var orchestrator = new BookLookupOrchestrator(
            [provider],
            NullLogger<BookLookupOrchestrator>.Instance);
        var metadataService = new IsbnMetadataService(orchestrator);
        var useCase = new AddBookUseCase(bookRepository, authorRepository, metadataService, _dbContext);
        var command = new AddBookByIsbnCommand(useCase);

        var exitCode = await command.ExecuteAsync(["9780306406157"], CancellationToken.None);

        Assert.Equal(0, exitCode);
        var book = await bookRepository.GetBookByIsbnAsync("9780306406157");
        Assert.NotNull(book);
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
    }

    private sealed class FakeProvider(BookMetadata? result) : IBookMetadataProvider
    {
        public string SourceName => "Fake";
        public Task<BookMetadata?> LookupAsync(string isbn, CancellationToken cancellationToken = default)
            => Task.FromResult(result);
    }
}
