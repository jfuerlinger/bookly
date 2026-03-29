namespace Bookly.Cli.Output;

public interface IFormatter<T>
{
    string Format(IEnumerable<T> items);
}
