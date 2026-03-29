using Bookly.Core.Data;
using Bookly.Core.Services;
using Bookly.Core.Tests.Builders;
using Bookly.Core.Tests.Fixtures;
using Bookly.Core.UseCases;
using Microsoft.EntityFrameworkCore;

namespace Bookly.Core.Tests.UseCases;

public class ListBooksUseCaseTests : IAsyncLifetime
{
    private BooklyDbContext _dbContext = null!;
    private BookRepository _bookRepository = null!;
    private ListBooksUseCase _useCase = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<BooklyDbContext>()
            .UseInMemoryDatabase(databaseName: $"bookly-list-usecase-{Guid.NewGuid()}")
            .Options;
        _dbContext = new BooklyDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
        _bookRepository = new BookRepository(_dbContext);
        _useCase = new ListBooksUseCase(_bookRepository);
    }

    public async Task DisposeAsync()
    {
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.DisposeAsync();
    }

    [Fact]
    public async Task ExecuteAsync_EmptyLibrary_ReturnsEmptyList()
    {
        var result = await _useCase.ExecuteAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task ExecuteAsync_WithBooks_ReturnsMappedDtos()
    {
        var book = new BookBuilder()
            .WithTitle("Test Title")
            .WithIsbn(TestData.ValidIsbn13)
            .WithAuthors("Author One", "Author Two")
            .Build();
        _dbContext.Books.Add(book);
        await _dbContext.SaveChangesAsync();

        var result = (await _useCase.ExecuteAsync()).ToList();

        Assert.Single(result);
        Assert.Equal("Test Title", result[0].Title);
        Assert.Contains("Author One", result[0].Authors);
        Assert.Contains("Author Two", result[0].Authors);
        Assert.Equal(TestData.ValidIsbn13, result[0].NormalizedIsbn);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsCoverUrls()
    {
        var book = TestData.SampleBook();
        book.CoverSmallUrl = "https://example.com/cover-s.jpg";
        book.CoverMediumUrl = "https://example.com/cover-m.jpg";
        book.CoverLargeUrl = "https://example.com/cover-l.jpg";
        _dbContext.Books.Add(book);
        await _dbContext.SaveChangesAsync();

        var result = (await _useCase.ExecuteAsync()).ToList();

        Assert.Equal("https://example.com/cover-s.jpg", result[0].CoverSmallUrl);
        Assert.Equal("https://example.com/cover-m.jpg", result[0].CoverMediumUrl);
        Assert.Equal("https://example.com/cover-l.jpg", result[0].CoverLargeUrl);
    }

    [Fact]
    public async Task ExecuteAsync_PaginationSkip_RespectsSkip()
    {
        for (var i = 0; i < 5; i++)
        {
            _dbContext.Books.Add(TestData.SampleBook($"978000000000{i}"));
        }
        await _dbContext.SaveChangesAsync();

        var allBooks = (await _useCase.ExecuteAsync(skip: 0, take: 10)).ToList();
        var pagedBooks = (await _useCase.ExecuteAsync(skip: 3, take: 10)).ToList();

        Assert.Equal(5, allBooks.Count);
        Assert.Equal(2, pagedBooks.Count);
    }

    [Fact]
    public async Task ExecuteAsync_PaginationTake_LimitsResults()
    {
        for (var i = 0; i < 5; i++)
        {
            _dbContext.Books.Add(TestData.SampleBook($"978000000000{i}"));
        }
        await _dbContext.SaveChangesAsync();

        var result = (await _useCase.ExecuteAsync(skip: 0, take: 3)).ToList();

        Assert.Equal(3, result.Count);
    }
}
