using Bookly.Core;
using Bookly.Core.Data;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<BooklyDbContext>("booklydb");
builder.Services.AddOpenApi();

var app = builder.Build();

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

app.Run();
