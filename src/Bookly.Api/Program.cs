using Bookly.Api.Endpoints;
using Bookly.Core;
using Bookly.Core.Data;
using Bookly.Core.Services;
using Bookly.Core.UseCases;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<BooklyDbContext>("booklydb");
builder.Services.AddOpenApi();

// External metadata providers (order = fallback priority)
builder.Services.AddHttpClient<OpenLibraryProvider>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddHttpClient<GoogleBooksProvider>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddTransient<IBookMetadataProvider>(sp =>
    sp.GetRequiredService<OpenLibraryProvider>());
builder.Services.AddTransient<IBookMetadataProvider>(sp =>
    sp.GetRequiredService<GoogleBooksProvider>());
builder.Services.AddScoped<BookLookupOrchestrator>();
builder.Services.AddScoped<IIsbnMetadataService, IsbnMetadataService>();
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();
builder.Services.AddScoped<IAddBookUseCase, AddBookUseCase>();
builder.Services.AddScoped<IListBooksUseCase, ListBooksUseCase>();

var app = builder.Build();

// Apply pending migrations on startup (development convenience)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<BooklyDbContext>();
    await db.Database.MigrateAsync();
}

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/docs", options =>
    {
        options.WithTitle("Bookly API");
    });
}

app.MapGet("/health", () => Results.Ok("healthy"));

app.MapGet("/api/add", (int a, int b) => Results.Ok(new { result = MathUtils.Add(a, b) }));

app.MapIsbnScanEndpoints();
app.MapListBooksEndpoints();

app.Run();
