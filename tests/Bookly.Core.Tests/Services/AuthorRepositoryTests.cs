using Bookly.Core.Data;
using Bookly.Core.Services;
using Bookly.Core.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace Bookly.Core.Tests.Services;

public class AuthorRepositoryTests : IAsyncLifetime
{
    private BooklyDbContext _dbContext = null!;
    private AuthorRepository _repository = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<BooklyDbContext>()
            .UseInMemoryDatabase(databaseName: $"bookly-author-test-{Guid.NewGuid()}")
            .Options;
        _dbContext = new BooklyDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
        _repository = new AuthorRepository(_dbContext);
    }

    [Fact]
    public async Task CreateAuthor_ValidAuthor_ReturnsAuthorWithId()
    {
        var author = TestData.SampleAuthor();
        var result = await _repository.CreateAuthorAsync(author);
        await _dbContext.SaveChangesAsync();
        Assert.NotEqual(0, result.Id);
        Assert.Equal("Test Author", result.Name);
    }

    [Fact]
    public async Task GetAuthorByName_ExistingName_ReturnsAuthor()
    {
        var author = TestData.SampleAuthor("Jane Doe");
        await _repository.CreateAuthorAsync(author);
        await _dbContext.SaveChangesAsync();
        var result = await _repository.GetAuthorByNameAsync("Jane Doe");
        Assert.NotNull(result);
        Assert.Equal("Jane Doe", result.Name);
    }

    [Fact]
    public async Task GetAuthorByName_NonExistent_ReturnsNull()
    {
        var result = await _repository.GetAuthorByNameAsync("Nobody Here");
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAuthors_Pagination_Works()
    {
        for (int i = 1; i <= 12; i++)
            await _repository.CreateAuthorAsync(TestData.SampleAuthor($"Author {i}"));
        await _dbContext.SaveChangesAsync();

        var page1 = (await _repository.GetAllAuthorsAsync(skip: 0, take: 10)).ToList();
        var page2 = (await _repository.GetAllAuthorsAsync(skip: 10, take: 10)).ToList();

        Assert.Equal(10, page1.Count);
        Assert.Equal(2, page2.Count);
        Assert.NotEqual(page1.First().Id, page2.First().Id);
    }

    [Fact]
    public async Task DeleteAuthor_ExistingId_RemovesAuthor()
    {
        var author = TestData.SampleAuthor("To Delete");
        var created = await _repository.CreateAuthorAsync(author);
        await _dbContext.SaveChangesAsync();

        var deleted = await _repository.DeleteAuthorAsync(created.Id);
        await _dbContext.SaveChangesAsync();

        Assert.True(deleted);
        var found = await _repository.GetAuthorByIdAsync(created.Id);
        Assert.Null(found);
    }

    [Fact]
    public async Task DeleteAuthor_NonExistentId_ReturnsFalse()
    {
        var deleted = await _repository.DeleteAuthorAsync(99999);
        Assert.False(deleted);
    }

    [Fact]
    public async Task UpdateAuthor_ChangesName()
    {
        var author = TestData.SampleAuthor("Original Name");
        var created = await _repository.CreateAuthorAsync(author);
        await _dbContext.SaveChangesAsync();

        created.Name = "Updated Name";
        await _repository.UpdateAuthorAsync(created);
        await _dbContext.SaveChangesAsync();

        var updated = await _repository.GetAuthorByIdAsync(created.Id);
        Assert.Equal("Updated Name", updated?.Name);
    }

    public async Task DisposeAsync()
    {
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.DisposeAsync();
    }
}
