using Bookly.Api.Models;
using Bookly.Api.Services;
using Bookly.Core.Data;
using Bookly.Core.Entities;
using Bookly.Core.Isbn;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Bookly.Api.Endpoints;

public static class IsbnScanEndpoint
{
    public static RouteGroupBuilder MapIsbnScanEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/library");

        group.MapPost("/isbn-scan", HandleScanAsync)
            .WithName("IsbnScan")
            .WithSummary("Scan an ISBN, fetch metadata, and save to library")
            .Produces<BookDto>(StatusCodes.Status200OK)
            .Produces<BookDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return group;
    }

    internal static async Task<Results<
        Created<BookDto>,
        Ok<BookDto>,
        ValidationProblem,
        ProblemHttpResult>> HandleScanAsync(
        IsbnScanRequest request,
        BookLookupOrchestrator orchestrator,
        BooklyDbContext db,
        ILogger<BookLookupOrchestrator> logger,
        CancellationToken cancellationToken)
    {
        // 1. Validate ISBN
        var validation = IsbnValidator.Validate(request.Isbn);
        if (!validation.IsValid)
        {
            return TypedResults.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["isbn"] = [validation.Error!]
                });
        }

        var normalizedIsbn = validation.NormalizedIsbn!;

        // 2. Check for existing book (deduplication)
        var existing = await db.Books
            .Include(b => b.BookAuthors)
            .ThenInclude(ba => ba.Author)
            .FirstOrDefaultAsync(b => b.NormalizedIsbn == normalizedIsbn, cancellationToken);

        if (existing is not null)
        {
            logger.LogInformation("Book with ISBN {Isbn} already exists (Id={BookId})",
                normalizedIsbn, existing.Id);
            return TypedResults.Ok(MapToDto(existing));
        }

        // 3. Lookup metadata
        var metadata = await orchestrator.LookupAsync(normalizedIsbn, cancellationToken);
        if (metadata is null)
        {
            return TypedResults.Problem(
                title: "Book not found",
                detail: $"No metadata found for ISBN '{request.Isbn}' from any provider.",
                statusCode: StatusCodes.Status404NotFound);
        }

        // 4. Resolve or create authors
        var bookAuthors = new List<BookAuthor>();
        foreach (var authorName in metadata.Authors)
        {
            var author = await db.Authors
                .FirstOrDefaultAsync(a => a.Name == authorName, cancellationToken);

            if (author is null)
            {
                author = new Author { Name = authorName };
                db.Authors.Add(author);
            }

            bookAuthors.Add(new BookAuthor { Author = author });
        }

        // 5. Create and persist book
        var now = DateTime.UtcNow;
        var book = new Book
        {
            Isbn10 = metadata.Isbn10 ?? validation.Isbn10,
            Isbn13 = metadata.Isbn13 ?? validation.Isbn13,
            NormalizedIsbn = normalizedIsbn,
            Title = metadata.Title,
            Subtitle = metadata.Subtitle,
            Publisher = metadata.Publisher,
            PublishedOn = metadata.PublishedOn,
            Language = metadata.Language,
            PageCount = metadata.PageCount,
            Description = metadata.Description,
            CoverSmallUrl = metadata.CoverSmallUrl,
            CoverMediumUrl = metadata.CoverMediumUrl,
            CoverLargeUrl = metadata.CoverLargeUrl,
            MetadataSource = metadata.Source,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            BookAuthors = bookAuthors,
        };

        db.Books.Add(book);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Saved book '{Title}' (Id={BookId}) from {Source}",
            book.Title, book.Id, book.MetadataSource);

        return TypedResults.Created($"/api/library/{book.Id}", MapToDto(book));
    }

    internal static BookDto MapToDto(Book book) => new()
    {
        Id = book.Id,
        Isbn10 = book.Isbn10,
        Isbn13 = book.Isbn13,
        NormalizedIsbn = book.NormalizedIsbn,
        Title = book.Title,
        Subtitle = book.Subtitle,
        Authors = book.BookAuthors.Select(ba => ba.Author.Name).ToList(),
        Publisher = book.Publisher,
        PublishedOn = book.PublishedOn,
        Language = book.Language,
        PageCount = book.PageCount,
        Description = book.Description,
        CoverSmallUrl = book.CoverSmallUrl,
        CoverMediumUrl = book.CoverMediumUrl,
        CoverLargeUrl = book.CoverLargeUrl,
        MetadataSource = book.MetadataSource,
        CreatedAtUtc = book.CreatedAtUtc,
        UpdatedAtUtc = book.UpdatedAtUtc,
    };
}
