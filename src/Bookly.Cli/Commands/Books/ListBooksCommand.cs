using Bookly.Cli.Output;
using Bookly.Core.UseCases;

namespace Bookly.Cli.Commands.Books;

public sealed class ListBooksCommand(IListBooksUseCase listBooksUseCase) : ICommand
{
    public string Name => "book list";

    public async Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken = default)
    {
        int skip = 0, take = 20;
        var format = "table";

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--skip" && i + 1 < args.Length && int.TryParse(args[++i], out var s)) skip = s;
            else if (args[i] == "--take" && i + 1 < args.Length && int.TryParse(args[++i], out var t)) take = t;
            else if (args[i] == "--output" && i + 1 < args.Length) format = args[++i];
        }

        try
        {
            var books = (await listBooksUseCase.ExecuteAsync(skip, take, cancellationToken)).ToList();
            var output = format switch
            {
                "json" => new JsonFormatter<Bookly.Core.Models.BookDto>().Format(books),
                "csv" => new BookCsvFormatter().Format(books),
                _ => new BookDtoTableFormatter().Format(books)
            };
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
