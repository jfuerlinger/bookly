using Bookly.Core.Entities;
using Bookly.Core.Tests.Fixtures;

namespace Bookly.Core.Tests.Builders;

public class BookBuilder
{
    private string _title = "Default Book";
    private string _normalizedIsbn = TestData.ValidIsbn13;
    private string _metadataSource = "test";
    private List<string> _authorNames = [];

    public BookBuilder WithTitle(string title) { _title = title; return this; }
    public BookBuilder WithIsbn(string isbn) { _normalizedIsbn = isbn; return this; }
    public BookBuilder WithSource(string source) { _metadataSource = source; return this; }
    public BookBuilder WithAuthors(params string[] names) { _authorNames = [..names]; return this; }

    public Book Build()
    {
        var book = new Book
        {
            Title = _title,
            NormalizedIsbn = _normalizedIsbn,
            MetadataSource = _metadataSource,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };

        foreach (var name in _authorNames)
            book.BookAuthors.Add(new BookAuthor
            {
                Author = new Author { Name = name },
                Book = book
            });

        return book;
    }
}
