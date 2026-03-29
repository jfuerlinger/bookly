using Bookly.Cli.Output;
using Bookly.Core.Services;

namespace Bookly.Cli.Commands.Books;

public sealed class GetBookCommand(IBookRepository repository) : ICommand
{
    public string Name => "book get";

    public async Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken = default)
    {
        if (args.Length < 1 || !int.TryParse(args[0], out var id))
        {
            Console.Error.WriteLine("Usage: bookly book get <id> [--output json|table]");
            return 1;
        }

        var outputFormat = "table";
        for (int i = 1; i < args.Length; i++)
            if (args[i] == "--output" && i + 1 < args.Length) outputFormat = args[++i];

        try
        {
            var book = await repository.GetBookByIdAsync(id, cancellationToken);
            if (book is null)
            {
                Console.Error.WriteLine($"Error: Book with ID {id} not found.");
                return 1;
            }

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
