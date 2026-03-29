using Bookly.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bookly.Core.Data;

public class BooklyDbContext(DbContextOptions<BooklyDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books => Set<Book>();
}
