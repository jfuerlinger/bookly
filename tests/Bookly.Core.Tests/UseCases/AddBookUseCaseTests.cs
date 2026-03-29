using Bookly.Core.Data;
using Bookly.Core.Entities;
using Bookly.Core.Models;
using Bookly.Core.Services;
using Bookly.Core.Tests.Fixtures;
using Bookly.Core.UseCases;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bookly.Core.Tests.UseCases;

public class AddBookUseCaseTests : IAsyncLifetime
{
    private BooklyDbContext _dbContext = null!;
    private BookRepository _bookRepository = null!;
    private AuthorRepository _authorRepository = null!;
    private AddBookUseCase _useCase = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<BooklyDbContext>()
            .UseInMemoryDatabase(databaseName: $"bookly-usecase-{Guid.NewGuid()}")
            .Options;
        _dbContext = new BooklyDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
        _bookRepository = new BookRepository(_dbContext);
        _authorRepository = new AuthorRepository(_dbContext);

        var metadata = new BookMetadata
        {
            Isbn13 = TestData.ValidIsbn13,
            Title = "Metadata Book",
            Authors = ["Meta Author"],
            Source = "test"
        };
        var provider = new FakeMetadataProvider(metadata);
        var orchestrator = new BookLookupOrchestrator(
            [provider],
            NullLogger<BookLookupOrchestrator>.Instance);
        var metadataService = new IsbnMetadataService(orchestrator);
        _useCase = new AddBookUseCase(_bookRepository, _authorRepository, metadataService, _dbContext);
    }

    [Fact]
    public async Task ExecuteAsync_ValidIsbn_ReturnsCreated()
    {
        var result = await _useCase.ExecuteAsync(TestData.ValidIsbn13);

        Assert.Equal(AddBookOutcome.Created, result.Outcome);
        Assert.NotNull(result.Book);
        Assert.NotEqual(0, result.Book.Id);
        var stored = await _bookRepository.GetBookByIsbnAsync(TestData.ValidIsbn13);
        Assert.NotNull(stored);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidIsbn_ReturnsValidationFailed()
    {
        var result = await _useCase.ExecuteAsync(TestData.InvalidIsbn);

        Assert.Equal(AddBookOutcome.ValidationFailed, result.Outcome);
        Assert.NotNull(result.Error);
        Assert.Null(result.Book);
    }

    [Fact]
    public async Task ExecuteAsync_DuplicateIsbn_ReturnsAlreadyExists()
    {
        var firstResult = await _useCase.ExecuteAsync(TestData.ValidIsbn13);
        var firstId = firstResult.Book!.Id;

        var secondResult = await _useCase.ExecuteAsync(TestData.ValidIsbn13);

        Assert.Equal(AddBookOutcome.AlreadyExists, secondResult.Outcome);
        Assert.Equal(firstId, secondResult.Book!.Id);
    }

    [Fact]
    public async Task ExecuteAsync_WithManualAuthors_CreatesAuthors()
    {
        var result = await _useCase.ExecuteAsync(
            TestData.ValidIsbn13,
            authorOverrides: ["Author One", "Author Two"]);

        Assert.Equal(AddBookOutcome.Created, result.Outcome);
        Assert.NotNull(result.Book);
        var authorNames = result.Book.BookAuthors.Select(ba => ba.Author!.Name).ToList();
        Assert.Contains("Author One", authorNames);
        Assert.Contains("Author Two", authorNames);
    }

    [Fact]
    public async Task ExecuteAsync_AuthorAlreadyExists_ReusesAuthor()
    {
        var existingAuthor = new Author { Name = "Existing Author" };
        await _authorRepository.CreateAuthorAsync(existingAuthor);
        await _dbContext.SaveChangesAsync();

        var result = await _useCase.ExecuteAsync(
            TestData.ValidIsbn13,
            authorOverrides: ["Existing Author"]);

        Assert.Equal(AddBookOutcome.Created, result.Outcome);
        Assert.NotNull(result.Book);
        var authorCount = await _dbContext.Authors.CountAsync(a => a.Name == "Existing Author");
        Assert.Equal(1, authorCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithTitleOverride_UsesTitleOverride()
    {
        var result = await _useCase.ExecuteAsync(
            TestData.ValidIsbn13,
            titleOverride: "Custom Title");

        Assert.Equal("Custom Title", result.Book!.Title);
    }

    [Fact]
    public async Task ExecuteAsync_NoMetadataNoOverrides_ReturnsMetadataNotFound()
    {
        // Create use case with provider that returns null
        var provider = new FakeMetadataProvider(null);
        var orchestrator = new BookLookupOrchestrator(
            [provider],
            NullLogger<BookLookupOrchestrator>.Instance);
        var metadataService = new IsbnMetadataService(orchestrator);
        var useCase = new AddBookUseCase(_bookRepository, _authorRepository, metadataService, _dbContext);

        var result = await useCase.ExecuteAsync(TestData.ValidIsbn13);

        Assert.Equal(AddBookOutcome.MetadataNotFound, result.Outcome);
        Assert.Null(result.Book);
    }

    public async Task DisposeAsync()
    {
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.DisposeAsync();
    }

    private sealed class FakeMetadataProvider(BookMetadata? result) : IBookMetadataProvider
    {
        public string SourceName => "Fake";
        public Task<BookMetadata?> LookupAsync(string isbn, CancellationToken cancellationToken = default)
            => Task.FromResult(result);
    }
}
