using Bookly.Cli.Output;
using Bookly.Core.Data;
using Bookly.Core.Services;

namespace Bookly.Cli.Commands.Books;

public sealed class UpdateBookCommand(IBookRepository repository, BooklyDbContext dbContext) : ICommand
{
    public string Name => "book update";

    public async Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken = default)
    {
        if (args.Length < 1 || !int.TryParse(args[0], out var id))
        {
            Console.Error.WriteLine("Usage: bookly book update <id> [--title <title>] [--subtitle <subtitle>] [--output json|table]");
            return 1;
        }

        string? title = null, subtitle = null;
        var outputFormat = "table";
        for (int i = 1; i < args.Length; i++)
        {
            if (args[i] == "--title" && i + 1 < args.Length) title = args[++i];
            else if (args[i] == "--subtitle" && i + 1 < args.Length) subtitle = args[++i];
            else if (args[i] == "--output" && i + 1 < args.Length) outputFormat = args[++i];
        }

        try
        {
            var book = await repository.GetBookByIdAsync(id, cancellationToken);
            if (book is null)
            {
                Console.Error.WriteLine($"Error: Book with ID {id} not found.");
                return 1;
            }

            if (title is not null) book.Title = title;
            if (subtitle is not null) book.Subtitle = subtitle;
            book.UpdatedAtUtc = DateTime.UtcNow;

            await repository.UpdateBookAsync(book, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            var output = outputFormat == "json"
                ? new JsonFormatter<Bookly.Core.Entities.Book>().Format([book])
                : new BookTableFormatter().Format([book]);
            Console.WriteLine(output);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"System error: {ex.Message}");
            return 2;
        }
    }
}
