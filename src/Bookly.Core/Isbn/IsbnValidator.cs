using System.Text.RegularExpressions;

namespace Bookly.Core.Isbn;

public static partial class IsbnValidator
{
    [GeneratedRegex(@"[\s\-]")]
    private static partial Regex SeparatorRegex();

    /// <summary>
    /// Strips hyphens and whitespace from a raw ISBN string.
    /// </summary>
    public static string Sanitize(string raw) =>
        SeparatorRegex().Replace(raw.Trim(), string.Empty);

    /// <summary>
    /// Validates an ISBN-10 string (must already be sanitized).
    /// </summary>
    public static bool IsValidIsbn10(string isbn)
    {
        if (isbn.Length != 10)
            return false;

        var sum = 0;
        for (var i = 0; i < 9; i++)
        {
            if (!char.IsAsciiDigit(isbn[i]))
                return false;
            sum += (isbn[i] - '0') * (10 - i);
        }

        var last = isbn[9];
        sum += last is 'X' or 'x' ? 10 : char.IsAsciiDigit(last) ? last - '0' : -1;

        return sum > 0 && sum % 11 == 0;
    }

    /// <summary>
    /// Validates an ISBN-13 string (must already be sanitized).
    /// </summary>
    public static bool IsValidIsbn13(string isbn)
    {
        if (isbn.Length != 13 || !isbn.All(char.IsAsciiDigit))
            return false;

        var sum = 0;
        for (var i = 0; i < 12; i++)
            sum += (isbn[i] - '0') * (i % 2 == 0 ? 1 : 3);

        var check = (10 - sum % 10) % 10;
        return check == isbn[12] - '0';
    }

    /// <summary>
    /// Converts an ISBN-10 to ISBN-13 (978 prefix).
    /// </summary>
    public static string? ConvertIsbn10ToIsbn13(string isbn10)
    {
        if (isbn10.Length != 10)
            return null;

        var prefix = "978" + isbn10[..9];
        var sum = 0;
        for (var i = 0; i < 12; i++)
            sum += (prefix[i] - '0') * (i % 2 == 0 ? 1 : 3);

        var check = (10 - sum % 10) % 10;
        return prefix + check;
    }

    /// <summary>
    /// Validates a raw ISBN string and returns a normalized result.
    /// </summary>
    public static IsbnValidationResult Validate(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return IsbnValidationResult.Invalid("ISBN is required.");

        var sanitized = Sanitize(raw);

        if (sanitized.Length == 13 && IsValidIsbn13(sanitized))
        {
            return IsbnValidationResult.ValidResult(
                isbn10: null,
                isbn13: sanitized,
                normalizedIsbn: sanitized);
        }

        if (sanitized.Length == 10 && IsValidIsbn10(sanitized))
        {
            var isbn13 = ConvertIsbn10ToIsbn13(sanitized);
            return IsbnValidationResult.ValidResult(
                isbn10: sanitized,
                isbn13: isbn13,
                normalizedIsbn: isbn13 ?? sanitized);
        }

        return IsbnValidationResult.Invalid(
            $"'{raw}' is not a valid ISBN-10 or ISBN-13.");
    }
}

public sealed record IsbnValidationResult
{
    public bool IsValid { get; private init; }
    public string? Isbn10 { get; private init; }
    public string? Isbn13 { get; private init; }
    public string? NormalizedIsbn { get; private init; }
    public string? Error { get; private init; }

    public static IsbnValidationResult ValidResult(string? isbn10, string? isbn13, string normalizedIsbn) =>
        new()
        {
            IsValid = true,
            Isbn10 = isbn10,
            Isbn13 = isbn13,
            NormalizedIsbn = normalizedIsbn,
        };

    public static IsbnValidationResult Invalid(string error) =>
        new() { IsValid = false, Error = error };
}
