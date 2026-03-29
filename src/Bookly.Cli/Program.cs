using Bookly.Core;

if (args.Length == 3 && args[0] == "add"
    && int.TryParse(args[1], out var a)
    && int.TryParse(args[2], out var b))
{
    Console.WriteLine($"{a} + {b} = {MathUtils.Add(a, b)}");
}
else
{
    Console.WriteLine("Usage: Bookly.Cli add <a> <b>");
    Console.WriteLine("Example: Bookly.Cli add 3 5");
}
