using Bookly.Api.Endpoints;
using Bookly.Core.Entities;
using Bookly.Core.Models;
using Bookly.Core.UseCases;
using Moq;

namespace Bookly.Api.Tests;

public class IsbnScanEndpointTests
{
    [Fact]
    public async Task HandleScanAsync_Created_ReturnsCreatedWithBookDto()
    {
        var book = CreateTestBook(1, "9780306406157", "Test Book", "TestSource");
        var useCase = new Mock<IAddBookUseCase>();
        useCase.Setup(u => u.ExecuteAsync(It.IsAny<string>(), null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AddBookResult { Outcome = AddBookOutcome.Created, Book = book });

        var request = new Api.Models.IsbnScanRequest { Isbn = "978-0-306-40615-7" };

        var result = await IsbnScanEndpoint.HandleScanAsync(request, useCase.Object, CancellationToken.None);

        var created = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.Created<BookDto>>(result.Result);
        Assert.Equal("Test Book", created.Value!.Title);
        Assert.Equal("9780306406157", created.Value.NormalizedIsbn);
        Assert.Equal("TestSource", created.Value.MetadataSource);
    }

    [Fact]
    public async Task HandleScanAsync_AlreadyExists_ReturnsOk()
    {
        var book = CreateTestBook(42, "9780306406157", "Existing Book", "Manual");
        var useCase = new Mock<IAddBookUseCase>();
        useCase.Setup(u => u.ExecuteAsync(It.IsAny<string>(), null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AddBookResult { Outcome = AddBookOutcome.AlreadyExists, Book = book });

        var request = new Api.Models.IsbnScanRequest { Isbn = "978-0-306-40615-7" };

        var result = await IsbnScanEndpoint.HandleScanAsync(request, useCase.Object, CancellationToken.None);

        var ok = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.Ok<BookDto>>(result.Result);
        Assert.Equal("Existing Book", ok.Value!.Title);
    }

    [Fact]
    public async Task HandleScanAsync_ValidationFailed_ReturnsValidationProblem()
    {
        var useCase = new Mock<IAddBookUseCase>();
        useCase.Setup(u => u.ExecuteAsync(It.IsAny<string>(), null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AddBookResult { Outcome = AddBookOutcome.ValidationFailed, Error = "Invalid ISBN" });

        var request = new Api.Models.IsbnScanRequest { Isbn = "invalid" };

        var result = await IsbnScanEndpoint.HandleScanAsync(request, useCase.Object, CancellationToken.None);

        Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.ValidationProblem>(result.Result);
    }

    [Fact]
    public async Task HandleScanAsync_MetadataNotFound_ReturnsProblem404()
    {
        var useCase = new Mock<IAddBookUseCase>();
        useCase.Setup(u => u.ExecuteAsync(It.IsAny<string>(), null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AddBookResult { Outcome = AddBookOutcome.MetadataNotFound, Error = "No metadata found" });

        var request = new Api.Models.IsbnScanRequest { Isbn = "978-0-306-40615-7" };

        var result = await IsbnScanEndpoint.HandleScanAsync(request, useCase.Object, CancellationToken.None);

        var problem = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult>(result.Result);
        Assert.Equal(404, problem.StatusCode);
    }

    private static Book CreateTestBook(int id, string isbn, string title, string source)
    {
        var author = new Author { Id = 1, Name = "Test Author" };
        var book = new Book
        {
            Id = id,
            NormalizedIsbn = isbn,
            Isbn13 = isbn,
            Title = title,
            MetadataSource = source,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
            BookAuthors = [new BookAuthor { BookId = id, AuthorId = 1, Author = author }]
        };
        return book;
    }
}
