using Bookly.Core.Models;
using Bookly.Core.UseCases;

namespace Bookly.Api.Endpoints;

public static class ListBooksEndpoint
{
    public static IEndpointRouteBuilder MapListBooksEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/api/library/books", HandleListAsync)
            .WithName("ListBooks")
            .WithSummary("List all books in the library with optional pagination")
            .Produces<IEnumerable<BookDto>>(StatusCodes.Status200OK);

        return routes;
    }

    internal static async Task<IResult> HandleListAsync(
        IListBooksUseCase useCase,
        int skip = 0,
        int take = 20,
        CancellationToken cancellationToken = default)
    {
        var books = await useCase.ExecuteAsync(skip, take, cancellationToken);
        return Results.Ok(books);
    }
}
