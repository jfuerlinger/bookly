using Bookly.Core.Entities;

namespace Bookly.Cli.Output;

public sealed class BookCsvFormatter : IFormatter<Book>
{
    public string Format(IEnumerable<Book> items)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Id,NormalizedIsbn,Title,Authors,Publisher,MetadataSource");
        foreach (var b in items)
        {
            var authors = string.Join(";", b.BookAuthors.Select(ba => ba.Author?.Name ?? ""));
            sb.AppendLine($"{b.Id},{b.NormalizedIsbn},{Escape(b.Title)},{Escape(authors)},{Escape(b.Publisher ?? "")},{b.MetadataSource}");
        }
        return sb.ToString().TrimEnd();
    }

    private static string Escape(string value) =>
        value.Contains(',') || value.Contains('"') || value.Contains('\n')
            ? $"\"{value.Replace("\"", "\"\"")}\"" 
            : value;
}
