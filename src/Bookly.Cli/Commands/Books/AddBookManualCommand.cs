using Bookly.Cli.Output;
using Bookly.Core.Data;
using Bookly.Core.Entities;
using Bookly.Core.Services;

namespace Bookly.Cli.Commands.Books;

public sealed class AddBookManualCommand(
    IBookRepository bookRepository,
    IAuthorRepository authorRepository,
    BooklyDbContext dbContext) : ICommand
{
    public string Name => "book add-manual";

    public async Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken = default)
    {
        if (args.Length < 1)
        {
            Console.Error.WriteLine("Usage: bookly book add-manual <title> --author <name> [--subtitle <subtitle>] [--output json|csv|table]");
            return 1;
        }

        var title = args[0].Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            Console.Error.WriteLine("Error: Title is required.");
            return 1;
        }

        var authorNames = new List<string>();
        string? subtitle = null;
        var outputFormat = "table";

        for (int i = 1; i < args.Length; i++)
        {
            if (args[i] == "--author" && i + 1 < args.Length) authorNames.Add(args[++i]);
            else if (args[i] == "--subtitle" && i + 1 < args.Length) subtitle = args[++i];
            else if (args[i] == "--output" && i + 1 < args.Length) outputFormat = args[++i];
        }

        if (authorNames.Count == 0)
        {
            Console.Error.WriteLine("Error: At least one --author is required.");
            return 1;
        }

        try
        {
            var bookAuthors = new List<BookAuthor>();
            foreach (var name in authorNames)
            {
                var author = await authorRepository.GetAuthorByNameAsync(name.Trim(), cancellationToken)
                    ?? await authorRepository.CreateAuthorAsync(new Author { Name = name.Trim() }, cancellationToken);
                bookAuthors.Add(new BookAuthor { Author = author });
            }

            var now = DateTime.UtcNow;
            var book = new Book
            {
                NormalizedIsbn = $"MANUAL{Guid.NewGuid():N}"[..13],
                Title = title,
                Subtitle = subtitle,
                MetadataSource = "manual",
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
                BookAuthors = bookAuthors,
            };

            await bookRepository.CreateBookAsync(book, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            var output = FormatBook(book, outputFormat);
            Console.WriteLine(output);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"System error: {ex.Message}");
            return 2;
        }
    }

    private static string FormatBook(Book book, string format) => format switch
    {
        "json" => new JsonFormatter<Book>().Format([book]),
        _ => new BookTableFormatter().Format([book])
    };
}
