using Bookly.Cli.Output;
using Bookly.Core.Entities;
using Bookly.Core.Models;

namespace Bookly.Cli.Tests.Output;

public class FormatterTests
{
    private static Book MakeBook(int id = 1, string title = "Test Book", string isbn = "9780306406157") => new()
    {
        Id = id,
        Title = title,
        NormalizedIsbn = isbn,
        MetadataSource = "test",
        CreatedAtUtc = DateTime.UtcNow,
        UpdatedAtUtc = DateTime.UtcNow,
    };

    private static BookDto MakeBookDto(int id = 1, string title = "Test Book", string isbn = "9780306406157") => new()
    {
        Id = id,
        Title = title,
        NormalizedIsbn = isbn,
        Authors = ["Test Author"],
        MetadataSource = "test",
    };

    [Fact]
    public void JsonFormatter_SingleBook_ReturnsValidJson()
    {
        var formatter = new JsonFormatter<Book>();
        var json = formatter.Format([MakeBook()]);
        Assert.NotEmpty(json);
        Assert.Contains("Test Book", json);
        Assert.Contains("9780306406157", json);
    }

    [Fact]
    public void JsonFormatter_EmptyList_ReturnsEmptyArray()
    {
        var formatter = new JsonFormatter<Book>();
        var json = formatter.Format([]);
        Assert.Equal("[]", json);
    }

    [Fact]
    public void TableFormatter_Book_ContainsTitleAndIsbn()
    {
        var formatter = new BookTableFormatter();
        var table = formatter.Format([MakeBook()]);
        Assert.Contains("Test Book", table);
        Assert.Contains("9780306406157", table);
        Assert.Contains("─", table);
    }

    [Fact]
    public void TableFormatter_EmptyList_ReturnsNoBooksMessage()
    {
        var formatter = new BookTableFormatter();
        var result = formatter.Format([]);
        Assert.Contains("No books found", result);
    }

    [Fact]
    public void CsvFormatter_Book_ContainsCsvHeaders()
    {
        var formatter = new BookCsvFormatter();
        var csv = formatter.Format([MakeBookDto()]);
        Assert.Contains("Id,", csv);
        Assert.Contains("Title", csv);
        Assert.Contains("Test Book", csv);
    }

    [Fact]
    public void AuthorTableFormatter_Author_ContainsIdAndName()
    {
        var author = new Author { Id = 1, Name = "Test Author" };
        var formatter = new AuthorTableFormatter();
        var table = formatter.Format([author]);
        Assert.Contains("Test Author", table);
    }
}
