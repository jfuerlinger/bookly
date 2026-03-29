using Bookly.Core.Data;
using Bookly.Core.Entities;
using Bookly.Core.Services;
using Bookly.Core.Tests.Fixtures;
using Bookly.Core.Tests.Services;
using Bookly.Core.UseCases;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Bookly.Core.Tests.UseCases;

public class AddBookUseCaseTests : IAsyncLifetime
{
    private BooklyDbContext _dbContext = null!;
    private BookRepository _bookRepository = null!;
    private AuthorRepository _authorRepository = null!;
    private IsbnMetadataService _metadataService = null!;
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

        var openLibraryResponse = $$"""
        {
          "ISBN:{{TestData.ValidIsbn13}}": {
            "title": "Metadata Book",
            "authors": [{"name": "Meta Author"}]
          }
        }
        """;
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, openLibraryResponse);
        _metadataService = new IsbnMetadataService(new HttpClient(handler), enableFallback: true);
        _useCase = new AddBookUseCase(_bookRepository, _authorRepository, _metadataService, _dbContext);
    }

    [Fact]
    public async Task ExecuteAsync_ValidIsbn_CreatesBook()
    {
        var result = await _useCase.ExecuteAsync(TestData.ValidIsbn13);
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
        var stored = await _bookRepository.GetBookByIsbnAsync(TestData.ValidIsbn13);
        Assert.NotNull(stored);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidIsbn_ThrowsInvalidOperationException()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _useCase.ExecuteAsync(TestData.InvalidIsbn));
    }

    [Fact]
    public async Task ExecuteAsync_DuplicateIsbn_UpdatesExisting()
    {
        var firstResult = await _useCase.ExecuteAsync(TestData.ValidIsbn13);
        var firstId = firstResult.Id;

        var secondResult = await _useCase.ExecuteAsync(TestData.ValidIsbn13);

        Assert.Equal(firstId, secondResult.Id); // Same book, not duplicate
    }

    [Fact]
    public async Task ExecuteAsync_WithManualAuthors_CreatesAuthors()
    {
        var result = await _useCase.ExecuteAsync(
            TestData.ValidIsbn13,
            authorOverrides: ["Author One", "Author Two"]);

        Assert.NotNull(result);
        var authorNames = result.BookAuthors.Select(ba => ba.Author!.Name).ToList();
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

        Assert.NotNull(result);
        var authorCount = await _dbContext.Authors.CountAsync(a => a.Name == "Existing Author");
        Assert.Equal(1, authorCount); // No duplicate authors
    }

    [Fact]
    public async Task ExecuteAsync_WithTitleOverride_UsesTitleOverride()
    {
        var result = await _useCase.ExecuteAsync(
            TestData.ValidIsbn13,
            titleOverride: "Custom Title");

        Assert.Equal("Custom Title", result.Title);
    }

    public async Task DisposeAsync()
    {
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.DisposeAsync();
    }
}
