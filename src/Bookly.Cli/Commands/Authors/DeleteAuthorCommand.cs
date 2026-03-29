using Bookly.Core.Data;
using Bookly.Core.Services;

namespace Bookly.Cli.Commands.Authors;

public sealed class DeleteAuthorCommand(IAuthorRepository repository, BooklyDbContext dbContext) : ICommand
{
    public string Name => "author delete";

    public async Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken = default)
    {
        if (args.Length < 1 || !int.TryParse(args[0], out var id))
        {
            Console.Error.WriteLine("Usage: bookly author delete <id> [--force]");
            return 1;
        }

        var force = args.Contains("--force");

        try
        {
            var author = await repository.GetAuthorByIdAsync(id, cancellationToken);
            if (author is null)
            {
                Console.Error.WriteLine($"Error: Author with ID {id} not found.");
                return 1;
            }

            if (!force)
            {
                Console.Write($"Delete '{author.Name}' (ID {id})? [y/N] ");
                var response = Console.ReadLine();
                if (!string.Equals(response, "y", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Aborted.");
                    return 0;
                }
            }

            await repository.DeleteAuthorAsync(id, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            Console.WriteLine($"Author '{author.Name}' (ID {id}) deleted.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"System error: {ex.Message}");
            return 2;
        }
    }
}
