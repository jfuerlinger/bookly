using Bookly.Cli.Output;
using Bookly.Core.Services;

namespace Bookly.Cli.Commands.Authors;

public sealed class GetAuthorCommand(IAuthorRepository repository) : ICommand
{
    public string Name => "author get";

    public async Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken = default)
    {
        if (args.Length < 1 || !int.TryParse(args[0], out var id))
        {
            Console.Error.WriteLine("Usage: bookly author get <id> [--output json|table]");
            return 1;
        }

        var outputFormat = "table";
        for (int i = 1; i < args.Length; i++)
            if (args[i] == "--output" && i + 1 < args.Length) outputFormat = args[++i];

        try
        {
            var author = await repository.GetAuthorByIdAsync(id, cancellationToken);
            if (author is null)
            {
                Console.Error.WriteLine($"Error: Author with ID {id} not found.");
                return 1;
            }

            var output = outputFormat == "json"
                ? new JsonFormatter<Bookly.Core.Entities.Author>().Format([author])
                : new AuthorTableFormatter().Format([author]);
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
