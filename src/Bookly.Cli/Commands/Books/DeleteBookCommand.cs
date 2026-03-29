using Bookly.Core.Data;
using Bookly.Core.Services;

namespace Bookly.Cli.Commands.Books;

public sealed class DeleteBookCommand(IBookRepository repository, BooklyDbContext dbContext) : ICommand
{
    public string Name => "book delete";

    public async Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken = default)
    {
        if (args.Length < 1 || !int.TryParse(args[0], out var id))
        {
            Console.Error.WriteLine("Usage: bookly book delete <id> [--force]");
            return 1;
        }

        var force = args.Contains("--force");

        try
        {
            var book = await repository.GetBookByIdAsync(id, cancellationToken);
            if (book is null)
            {
                Console.Error.WriteLine($"Error: Book with ID {id} not found.");
                return 1;
            }

            if (!force)
            {
                Console.Write($"Delete '{book.Title}' (ID {id})? [y/N] ");
                var response = Console.ReadLine();
                if (!string.Equals(response, "y", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Aborted.");
                    return 0;
                }
            }

            await repository.DeleteBookAsync(id, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            Console.WriteLine($"Book '{book.Title}' (ID {id}) deleted.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"System error: {ex.Message}");
            return 2;
        }
    }
}
