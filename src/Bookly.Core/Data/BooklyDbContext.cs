using Bookly.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bookly.Core.Data;

public class BooklyDbContext(DbContextOptions<BooklyDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<BookAuthor> BookAuthors => Set<BookAuthor>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BooklyDbContext).Assembly);
    }
}
