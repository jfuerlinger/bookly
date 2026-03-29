using Bookly.Cli.Output;
using Bookly.Core.Services;

namespace Bookly.Cli.Commands.Authors;

public sealed class ListAuthorsCommand(IAuthorRepository repository) : ICommand
{
    public string Name => "author list";

    public async Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken = default)
    {
        int skip = 0, take = 10;
        var format = "table";

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--skip" && i + 1 < args.Length && int.TryParse(args[++i], out var s)) skip = s;
            else if (args[i] == "--take" && i + 1 < args.Length && int.TryParse(args[++i], out var t)) take = t;
            else if (args[i] == "--output" && i + 1 < args.Length) format = args[++i];
        }

        try
        {
            var authors = (await repository.GetAllAuthorsAsync(skip, take, cancellationToken)).ToList();
            var output = format == "json"
                ? new JsonFormatter<Bookly.Core.Entities.Author>().Format(authors)
                : new AuthorTableFormatter().Format(authors);
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
