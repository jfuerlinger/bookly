using Bookly.Core.Data;
using Bookly.Core.Entities;
using Bookly.Core.Services;
using Bookly.Core.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace Bookly.Core.Tests.Services;

public class BookRepositoryTests : IAsyncLifetime
{
    private BooklyDbContext _dbContext = null!;
    private BookRepository _repository = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<BooklyDbContext>()
            .UseInMemoryDatabase(databaseName: $"bookly-test-{Guid.NewGuid()}")
            .Options;
        _dbContext = new BooklyDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
        _repository = new BookRepository(_dbContext);
    }

    [Fact]
    public async Task CreateBook_ValidBook_ReturnsBookWithId()
    {
        var book = TestData.SampleBook();
        var result = await _repository.CreateBookAsync(book);
        await _dbContext.SaveChangesAsync();
        Assert.NotEqual(0, result.Id);
        Assert.Equal("Sample Book", result.Title);
    }

    [Fact]
    public async Task GetBookByIsbn_ExistingIsbn_ReturnsBook()
    {
        var book = TestData.SampleBook();
        await _repository.CreateBookAsync(book);
        await _dbContext.SaveChangesAsync();
        var result = await _repository.GetBookByIsbnAsync(TestData.ValidIsbn13);
        Assert.NotNull(result);
        Assert.Equal("Sample Book", result.Title);
    }

    [Fact]
    public async Task GetBookByIsbn_NonExistentIsbn_ReturnsNull()
    {
        var result = await _repository.GetBookByIsbnAsync("9999999999999");
        Assert.Null(result);
    }

    [Fact]
    public async Task GetBookById_ExistingId_ReturnsBook()
    {
        var book = TestData.SampleBook();
        var created = await _repository.CreateBookAsync(book);
        await _dbContext.SaveChangesAsync();
        var result = await _repository.GetBookByIdAsync(created.Id);
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
    }

    [Fact]
    public async Task GetBookById_NonExistentId_ReturnsNull()
    {
        var result = await _repository.GetBookByIdAsync(99999);
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteBook_ExistingId_RemovesBook()
    {
        var book = TestData.SampleBook();
        var created = await _repository.CreateBookAsync(book);
        await _dbContext.SaveChangesAsync();
        var deleted = await _repository.DeleteBookAsync(created.Id);
        await _dbContext.SaveChangesAsync();
        Assert.True(deleted);
        var found = await _repository.GetBookByIdAsync(created.Id);
        Assert.Null(found);
    }

    [Fact]
    public async Task DeleteBook_NonExistentId_ReturnsFalse()
    {
        var deleted = await _repository.DeleteBookAsync(99999);
        Assert.False(deleted);
    }

    [Fact]
    public async Task GetAllBooks_Pagination_SkipAndTake()
    {
        for (int i = 1; i <= 15; i++)
        {
            await _repository.CreateBookAsync(new Book
            {
                Title = $"Book {i}",
                NormalizedIsbn = $"978000000{i:D4}",
                MetadataSource = "test",
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
            });
        }
        await _dbContext.SaveChangesAsync();
        var page1 = (await _repository.GetAllBooksAsync(skip: 0, take: 10)).ToList();
        var page2 = (await _repository.GetAllBooksAsync(skip: 10, take: 10)).ToList();
        Assert.Equal(10, page1.Count);
        Assert.Equal(5, page2.Count);
        Assert.NotEqual(page1.First().Id, page2.First().Id);
    }

    [Fact]
    public async Task UpdateBook_ChangesTitle()
    {
        var book = TestData.SampleBook();
        var created = await _repository.CreateBookAsync(book);
        await _dbContext.SaveChangesAsync();
        created.Title = "Updated Title";
        await _repository.UpdateBookAsync(created);
        await _dbContext.SaveChangesAsync();
        var updated = await _repository.GetBookByIdAsync(created.Id);
        Assert.Equal("Updated Title", updated?.Title);
    }

    public async Task DisposeAsync()
    {
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.DisposeAsync();
    }
}
