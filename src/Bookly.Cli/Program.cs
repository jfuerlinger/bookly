using Bookly.Cli.Commands;
using Bookly.Cli.Commands.Authors;
using Bookly.Cli.Commands.Books;
using Bookly.Cli.Handlers;
using Bookly.Core.Data;
using Bookly.Core.Services;
using Bookly.Core.UseCases;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables("BOOKLY_");

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration["Bookly:Database:ConnectionString"]
    ?? "Host=localhost;Database=bookly_dev;Username=postgres;Password=postgres";

builder.Services.AddDbContextPool<BooklyDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();

builder.Services.AddHttpClient<OpenLibraryProvider>(c => c.Timeout = TimeSpan.FromSeconds(10));
builder.Services.AddHttpClient<GoogleBooksProvider>(c => c.Timeout = TimeSpan.FromSeconds(10));
builder.Services.AddTransient<IBookMetadataProvider>(sp => sp.GetRequiredService<OpenLibraryProvider>());
builder.Services.AddTransient<IBookMetadataProvider>(sp => sp.GetRequiredService<GoogleBooksProvider>());
builder.Services.AddScoped<BookLookupOrchestrator>();
builder.Services.AddScoped<IIsbnMetadataService, IsbnMetadataService>();

builder.Services.AddScoped<IAddBookUseCase, AddBookUseCase>();
builder.Services.AddScoped<IListBooksUseCase, ListBooksUseCase>();

builder.Services.AddScoped<ICommand, AddBookByIsbnCommand>();
builder.Services.AddScoped<ICommand, AddBookManualCommand>();
builder.Services.AddScoped<ICommand, ListBooksCommand>();
builder.Services.AddScoped<ICommand, GetBookCommand>();
builder.Services.AddScoped<ICommand, UpdateBookCommand>();
builder.Services.AddScoped<ICommand, DeleteBookCommand>();
builder.Services.AddScoped<ICommand, AddAuthorCommand>();
builder.Services.AddScoped<ICommand, ListAuthorsCommand>();
builder.Services.AddScoped<ICommand, GetAuthorCommand>();
builder.Services.AddScoped<ICommand, UpdateAuthorCommand>();
builder.Services.AddScoped<ICommand, DeleteAuthorCommand>();
builder.Services.AddScoped<CommandHandler>();

var host = builder.Build();

using var scope = host.Services.CreateScope();
var handler = scope.ServiceProvider.GetRequiredService<CommandHandler>();
var exitCode = await handler.HandleAsync(args);
Environment.Exit(exitCode);
