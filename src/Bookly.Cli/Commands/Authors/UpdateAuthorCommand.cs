using Bookly.Cli.Output;
using Bookly.Core.Data;
using Bookly.Core.Services;

namespace Bookly.Cli.Commands.Authors;

public sealed class UpdateAuthorCommand(IAuthorRepository repository, BooklyDbContext dbContext) : ICommand
{
    public string Name => "author update";

    public async Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken = default)
    {
        if (args.Length < 1 || !int.TryParse(args[0], out var id))
        {
            Console.Error.WriteLine("Usage: bookly author update <id> --name <name> [--output json|table]");
            return 1;
        }

        string? name = null;
        var outputFormat = "table";
        for (int i = 1; i < args.Length; i++)
        {
            if (args[i] == "--name" && i + 1 < args.Length) name = args[++i];
            else if (args[i] == "--output" && i + 1 < args.Length) outputFormat = args[++i];
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            Console.Error.WriteLine("Error: --name is required.");
            return 1;
        }

        try
        {
            var author = await repository.GetAuthorByIdAsync(id, cancellationToken);
            if (author is null)
            {
                Console.Error.WriteLine($"Error: Author with ID {id} not found.");
                return 1;
            }

            author.Name = name.Trim();
            await repository.UpdateAuthorAsync(author, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

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
