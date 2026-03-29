using Bookly.Core.Models;
using Bookly.Core.Services;

namespace Bookly.Core.UseCases;

public sealed class ListBooksUseCase(IBookRepository bookRepository) : IListBooksUseCase
{
    public async Task<IEnumerable<BookDto>> ExecuteAsync(
        int skip = 0,
        int take = 20,
        CancellationToken cancellationToken = default)
    {
        var books = await bookRepository.GetAllBooksAsync(skip, take, cancellationToken);
        return books.Select(b => new BookDto
        {
            Id = b.Id,
            Isbn10 = b.Isbn10,
            Isbn13 = b.Isbn13,
            NormalizedIsbn = b.NormalizedIsbn,
            Title = b.Title,
            Subtitle = b.Subtitle,
            Authors = b.BookAuthors.Select(ba => ba.Author.Name).ToList(),
            Publisher = b.Publisher,
            PublishedOn = b.PublishedOn,
            Language = b.Language,
            PageCount = b.PageCount,
            Description = b.Description,
            CoverSmallUrl = b.CoverSmallUrl,
            CoverMediumUrl = b.CoverMediumUrl,
            CoverLargeUrl = b.CoverLargeUrl,
            MetadataSource = b.MetadataSource,
            CreatedAtUtc = b.CreatedAtUtc,
            UpdatedAtUtc = b.UpdatedAtUtc,
        });
    }
}
