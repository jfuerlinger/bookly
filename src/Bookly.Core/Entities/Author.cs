namespace Bookly.Core.Entities;

public class Author
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public ICollection<BookAuthor> BookAuthors { get; set; } = [];
}
