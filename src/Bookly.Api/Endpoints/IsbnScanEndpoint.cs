using Bookly.Api.Models;
using Bookly.Core.Entities;
using Bookly.Core.Models;
using Bookly.Core.UseCases;
using Microsoft.AspNetCore.Http.HttpResults;

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
        IAddBookUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(request.Isbn ?? "", cancellationToken: cancellationToken);

        return result.Outcome switch
        {
            AddBookOutcome.ValidationFailed => TypedResults.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["isbn"] = [result.Error!]
                }),

            AddBookOutcome.AlreadyExists => TypedResults.Ok(MapToDto(result.Book!)),

            AddBookOutcome.MetadataNotFound => TypedResults.Problem(
                title: "Book not found",
                detail: result.Error,
                statusCode: StatusCodes.Status404NotFound),

            AddBookOutcome.Created => TypedResults.Created(
                $"/api/library/{result.Book!.Id}", MapToDto(result.Book)),

            _ => TypedResults.Problem(
                title: "Unexpected error",
                statusCode: StatusCodes.Status500InternalServerError)
        };
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
