using Bookly.Core.Entities;
using Bookly.Core.Models;

namespace Bookly.Cli.Output;

public sealed class BookTableFormatter : IFormatter<Book>
{
    public string Format(IEnumerable<Book> items)
    {
        var books = items.ToList();
        if (books.Count == 0)
            return "No books found.";

        var rows = books.Select(b => new[]
        {
            b.Id.ToString(),
            b.NormalizedIsbn,
            b.Title.Length > 40 ? b.Title[..37] + "..." : b.Title,
            string.Join(", ", b.BookAuthors.Select(ba => ba.Author?.Name ?? ""))
        }).ToList();

        return FormatTable(["Id", "ISBN", "Title", "Authors"], rows);
    }

    internal static string FormatTable(string[] headers, List<string[]> rows)
    {
        var colWidths = headers.Select((h, i) =>
            Math.Max(h.Length, rows.Count > 0 ? rows.Max(r => r.Length > i ? r[i].Length : 0) : 0)
        ).ToArray();

        var sb = new System.Text.StringBuilder();
        var top = "┌" + string.Join("┬", colWidths.Select(w => new string('─', w + 2))) + "┐";
        var mid = "├" + string.Join("┼", colWidths.Select(w => new string('─', w + 2))) + "┤";
        var bot = "└" + string.Join("┴", colWidths.Select(w => new string('─', w + 2))) + "┘";

        sb.AppendLine(top);
        sb.AppendLine("│" + string.Join("│", headers.Select((h, i) => $" {h.PadRight(colWidths[i])} ")) + "│");
        if (rows.Count > 0) sb.AppendLine(mid);
        foreach (var row in rows)
            sb.AppendLine("│" + string.Join("│", row.Select((c, i) => $" {c.PadRight(colWidths[i])} ")) + "│");
        sb.Append(bot);
        return sb.ToString();
    }
}

public sealed class BookDtoTableFormatter : IFormatter<BookDto>
{
    public string Format(IEnumerable<BookDto> items)
    {
        var books = items.ToList();
        if (books.Count == 0)
            return "No books found.";

        var rows = books.Select(b => new[]
        {
            b.Id.ToString(),
            b.NormalizedIsbn,
            b.Title.Length > 40 ? b.Title[..37] + "..." : b.Title,
            string.Join(", ", b.Authors)
        }).ToList();

        return BookTableFormatter.FormatTable(["Id", "ISBN", "Title", "Authors"], rows);
    }
}

public sealed class AuthorTableFormatter : IFormatter<Author>
{
    public string Format(IEnumerable<Author> items)
    {
        var authors = items.ToList();
        if (authors.Count == 0)
            return "No authors found.";

        var rows = authors.Select(a => new[] { a.Id.ToString(), a.Name }).ToList();
        return BookTableFormatter.FormatTable(["Id", "Name"], rows);
    }
}
