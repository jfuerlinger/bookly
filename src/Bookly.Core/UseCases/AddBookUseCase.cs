using Bookly.Core.Data;
using Bookly.Core.Entities;
using Bookly.Core.Isbn;
using Bookly.Core.Services;

namespace Bookly.Core.UseCases;

public sealed class AddBookUseCase : IAddBookUseCase
{
    private readonly IBookRepository _bookRepository;
    private readonly IAuthorRepository _authorRepository;
    private readonly IIsbnMetadataService _metadataService;
    private readonly BooklyDbContext _dbContext;

    public AddBookUseCase(
        IBookRepository bookRepository,
        IAuthorRepository authorRepository,
        IIsbnMetadataService metadataService,
        BooklyDbContext dbContext)
    {
        _bookRepository = bookRepository;
        _authorRepository = authorRepository;
        _metadataService = metadataService;
        _dbContext = dbContext;
    }

    public async Task<AddBookResult> ExecuteAsync(
        string isbn,
        string? titleOverride = null,
        IEnumerable<string>? authorOverrides = null,
        CancellationToken cancellationToken = default)
    {
        var validation = IsbnValidator.Validate(isbn);
        if (!validation.IsValid)
            return new AddBookResult { Outcome = AddBookOutcome.ValidationFailed, Error = validation.Error };

        var normalizedIsbn = validation.NormalizedIsbn!;

        var existing = await _bookRepository.GetBookByIsbnAsync(normalizedIsbn, cancellationToken);
        if (existing is not null)
        {
            if (titleOverride is not null)
            {
                existing.Title = titleOverride;
                existing.UpdatedAtUtc = DateTime.UtcNow;
                await _bookRepository.UpdateBookAsync(existing, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            return new AddBookResult { Outcome = AddBookOutcome.AlreadyExists, Book = existing };
        }

        var metadata = await _metadataService.ResolveIsbnAsync(isbn, cancellationToken);

        var hasOverrides = titleOverride is not null || authorOverrides?.Any() == true;
        if (metadata is null && !hasOverrides)
            return new AddBookResult { Outcome = AddBookOutcome.MetadataNotFound, Error = $"No metadata found for ISBN '{isbn}' from any provider." };

        var title = titleOverride ?? metadata?.Title ?? normalizedIsbn;
        var source = metadata?.Source ?? "manual";

        var authorNames = authorOverrides?.ToList() ?? metadata?.Authors ?? [];
        var bookAuthors = new List<BookAuthor>();
        foreach (var authorName in authorNames)
        {
            if (string.IsNullOrWhiteSpace(authorName))
                continue;

            var author = await _authorRepository.GetAuthorByNameAsync(authorName.Trim(), cancellationToken);
            if (author is null)
            {
                author = new Author { Name = authorName.Trim() };
                await _authorRepository.CreateAuthorAsync(author, cancellationToken);
            }
            bookAuthors.Add(new BookAuthor { Author = author });
        }

        var now = DateTime.UtcNow;
        var book = new Book
        {
            Isbn10 = metadata?.Isbn10 ?? validation.Isbn10,
            Isbn13 = metadata?.Isbn13 ?? validation.Isbn13,
            NormalizedIsbn = normalizedIsbn,
            Title = title,
            Subtitle = metadata?.Subtitle,
            Publisher = metadata?.Publisher,
            PublishedOn = metadata?.PublishedOn,
            Language = metadata?.Language,
            PageCount = metadata?.PageCount,
            Description = metadata?.Description,
            CoverSmallUrl = metadata?.CoverSmallUrl,
            CoverMediumUrl = metadata?.CoverMediumUrl,
            CoverLargeUrl = metadata?.CoverLargeUrl,
            MetadataSource = source,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            BookAuthors = bookAuthors,
        };

        await _bookRepository.CreateBookAsync(book, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AddBookResult
        {
            Outcome = AddBookOutcome.Created,
            Book = (await _bookRepository.GetBookByIsbnAsync(normalizedIsbn, cancellationToken))!
        };
    }
}
