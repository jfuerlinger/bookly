using Bookly.Cli.Output;
using Bookly.Core.UseCases;

namespace Bookly.Cli.Commands.Books;

public sealed class AddBookByIsbnCommand(IAddBookUseCase useCase) : ICommand
{
    public string Name => "book add-by-isbn";

    public async Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken = default)
    {
        if (args.Length < 1)
        {
            Console.Error.WriteLine("Usage: bookly book add-by-isbn <isbn> [--title <title>] [--author <name>] [--output json|csv|table]");
            return 1;
        }

        var isbn = args[0];
        string? titleOverride = null;
        var authorOverrides = new List<string>();
        var outputFormat = "table";

        for (int i = 1; i < args.Length; i++)
        {
            if (args[i] == "--title" && i + 1 < args.Length) titleOverride = args[++i];
            else if (args[i] == "--author" && i + 1 < args.Length) authorOverrides.Add(args[++i]);
            else if (args[i] == "--output" && i + 1 < args.Length) outputFormat = args[++i];
        }

        try
        {
            var result = await useCase.ExecuteAsync(isbn, titleOverride, authorOverrides.Count > 0 ? authorOverrides : null, cancellationToken);

            switch (result.Outcome)
            {
                case AddBookOutcome.ValidationFailed:
                    Console.Error.WriteLine($"Error: {result.Error}");
                    return 1;

                case AddBookOutcome.MetadataNotFound:
                    Console.Error.WriteLine($"Error: {result.Error}");
                    return 1;

                case AddBookOutcome.AlreadyExists:
                case AddBookOutcome.Created:
                    var output = FormatBook(result.Book!, outputFormat);
                    Console.WriteLine(output);
                    return 0;

                default:
                    Console.Error.WriteLine("Unexpected error.");
                    return 2;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"System error: {ex.Message}");
            return 2;
        }
    }

    private static string FormatBook(Bookly.Core.Entities.Book book, string format) => format switch
    {
        "json" => new JsonFormatter<Bookly.Core.Entities.Book>().Format([book]),
        _ => new BookTableFormatter().Format([book])
    };
}
