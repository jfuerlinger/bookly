namespace Bookly.Cli.Handlers;

public sealed class CommandHandler(IEnumerable<Bookly.Cli.Commands.ICommand> commands)
{
    public async Task<int> HandleAsync(string[] args, CancellationToken cancellationToken = default)
    {
        if (args.Length < 2)
        {
            PrintHelp();
            return 1;
        }

        var commandName = $"{args[0]} {args[1]}";
        var command = commands.FirstOrDefault(c => c.Name == commandName);

        if (command is null)
        {
            Console.Error.WriteLine($"Unknown command: {commandName}");
            PrintHelp();
            return 1;
        }

        var remainingArgs = args[2..];
        return await command.ExecuteAsync(remainingArgs, cancellationToken);
    }

    private static void PrintHelp()
    {
        Console.WriteLine("Usage: bookly <resource> <command> [options]");
        Console.WriteLine();
        Console.WriteLine("Book commands:");
        Console.WriteLine("  bookly book add-by-isbn <isbn>");
        Console.WriteLine("  bookly book add-manual <title> --author <name>");
        Console.WriteLine("  bookly book list [--skip N] [--take N] [--output json|csv|table]");
        Console.WriteLine("  bookly book get <id>");
        Console.WriteLine("  bookly book update <id> [--title <title>]");
        Console.WriteLine("  bookly book delete <id> [--force]");
        Console.WriteLine();
        Console.WriteLine("Author commands:");
        Console.WriteLine("  bookly author add <name>");
        Console.WriteLine("  bookly author list [--skip N] [--take N]");
        Console.WriteLine("  bookly author get <id>");
        Console.WriteLine("  bookly author update <id> --name <name>");
        Console.WriteLine("  bookly author delete <id> [--force]");
    }
}
