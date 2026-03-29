using Bookly.Cli.Output;
using Bookly.Core.Data;
using Bookly.Core.Entities;
using Bookly.Core.Services;

namespace Bookly.Cli.Commands.Authors;

public sealed class AddAuthorCommand(IAuthorRepository repository, BooklyDbContext dbContext) : ICommand
{
    public string Name => "author add";

    public async Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken = default)
    {
        if (args.Length < 1 || string.IsNullOrWhiteSpace(args[0]))
        {
            Console.Error.WriteLine("Usage: bookly author add <name> [--output json|table]");
            return 1;
        }

        var name = args[0].Trim();
        var outputFormat = "table";
        for (int i = 1; i < args.Length; i++)
            if (args[i] == "--output" && i + 1 < args.Length) outputFormat = args[++i];

        try
        {
            var existing = await repository.GetAuthorByNameAsync(name, cancellationToken);
            if (existing is not null)
            {
                Console.Error.WriteLine($"Error: Author '{name}' already exists.");
                return 1;
            }

            var author = new Author { Name = name };
            await repository.CreateAuthorAsync(author, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            var output = outputFormat == "json"
                ? new JsonFormatter<Author>().Format([author])
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
