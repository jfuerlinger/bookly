using Bookly.Core.Entities;

namespace Bookly.Core.Tests.Builders;

public class AuthorBuilder
{
    private string _name = "Default Author";
    public AuthorBuilder WithName(string name) { _name = name; return this; }
    public Author Build() => new() { Name = _name };
}
