using Bookly.Cli.Commands.Books;
using Bookly.Core.Models;
using Bookly.Core.UseCases;
using Moq;

namespace Bookly.Cli.Tests.Commands;

public class ListBooksCommandTests
{
    private readonly Mock<IListBooksUseCase> _useCase = new();
    private readonly ListBooksCommand _command;

    public ListBooksCommandTests()
    {
        _command = new ListBooksCommand(_useCase.Object);
    }

    [Fact]
    public async Task ListBooks_EmptyResult_ExitCode0()
    {
        _useCase.Setup(u => u.ExecuteAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var exitCode = await _command.ExecuteAsync([], CancellationToken.None);

        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task ListBooks_WithBooks_ExitCode0()
    {
        _useCase.Setup(u => u.ExecuteAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([MakeBook("Test Book", "9780306406157", ["Author One"])]);

        var exitCode = await _command.ExecuteAsync([], CancellationToken.None);

        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task ListBooks_JsonFormat_ExitCode0()
    {
        _useCase.Setup(u => u.ExecuteAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var exitCode = await _command.ExecuteAsync(["--output", "json"], CancellationToken.None);

        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task ListBooks_PaginationArgs_PassedToUseCase()
    {
        _useCase.Setup(u => u.ExecuteAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await _command.ExecuteAsync(["--skip", "5", "--take", "15"], CancellationToken.None);

        _useCase.Verify(u => u.ExecuteAsync(5, 15, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ListBooks_UseCaseThrows_ReturnsExitCode2()
    {
        _useCase.Setup(u => u.ExecuteAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("db error"));

        var exitCode = await _command.ExecuteAsync([], CancellationToken.None);

        Assert.Equal(2, exitCode);
    }

    private static BookDto MakeBook(string title, string isbn, List<string> authors) => new()
    {
        Id = 1,
        Title = title,
        NormalizedIsbn = isbn,
        Authors = authors,
        MetadataSource = "test",
    };
}


