namespace Bookly.Core.UseCases;

public interface IAddBookUseCase
{
    Task<AddBookResult> ExecuteAsync(
        string isbn,
        string? titleOverride = null,
        IEnumerable<string>? authorOverrides = null,
        CancellationToken cancellationToken = default);
}
