using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bookly.Cli.Output;

public sealed class JsonFormatter<T> : IFormatter<T>
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public string Format(IEnumerable<T> items) =>
        JsonSerializer.Serialize(items, Options);
}
