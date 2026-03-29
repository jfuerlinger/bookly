using Bookly.Core.Isbn;

namespace Bookly.Core.Tests;

public class IsbnValidatorTests
{
    // --- ISBN-13 validation ---

    [Theory]
    [InlineData("9780306406157", true)]  // valid ISBN-13
    [InlineData("9783161484100", true)]  // valid ISBN-13
    [InlineData("9780140449136", true)]  // valid ISBN-13 (The Republic, Plato)
    [InlineData("9780306406158", false)] // invalid check digit
    [InlineData("978030640615", false)]  // too short
    [InlineData("97803064061570", false)] // too long
    [InlineData("978030640615X", false)] // X not allowed in ISBN-13
    [InlineData("abcdefghijklm", false)] // letters
    public void IsValidIsbn13_ReturnsExpected(string isbn, bool expected)
    {
        Assert.Equal(expected, IsbnValidator.IsValidIsbn13(isbn));
    }

    // --- ISBN-10 validation ---

    [Theory]
    [InlineData("0306406152", true)]   // valid ISBN-10
    [InlineData("0140449132", true)]   // valid ISBN-10
    [InlineData("080442957X", true)]   // valid ISBN-10 with X check digit
    [InlineData("0306406153", false)]  // invalid check digit
    [InlineData("030640615", false)]   // too short
    [InlineData("03064061520", false)] // too long
    [InlineData("abcdefghij", false)]  // letters
    public void IsValidIsbn10_ReturnsExpected(string isbn, bool expected)
    {
        Assert.Equal(expected, IsbnValidator.IsValidIsbn10(isbn));
    }

    // --- ISBN-10 to ISBN-13 conversion ---

    [Theory]
    [InlineData("0306406152", "9780306406157")]
    [InlineData("0140449132", "9780140449136")]
    public void ConvertIsbn10ToIsbn13_ReturnsCorrectIsbn13(string isbn10, string expectedIsbn13)
    {
        Assert.Equal(expectedIsbn13, IsbnValidator.ConvertIsbn10ToIsbn13(isbn10));
    }

    [Fact]
    public void ConvertIsbn10ToIsbn13_WrongLength_ReturnsNull()
    {
        Assert.Null(IsbnValidator.ConvertIsbn10ToIsbn13("123"));
    }

    // --- Sanitize ---

    [Theory]
    [InlineData("978-0-306-40615-7", "9780306406157")]
    [InlineData("978 0 306 40615 7", "9780306406157")]
    [InlineData("  978-0-306-40615-7  ", "9780306406157")]
    [InlineData("0-306-40615-2", "0306406152")]
    public void Sanitize_RemovesHyphensAndWhitespace(string raw, string expected)
    {
        Assert.Equal(expected, IsbnValidator.Sanitize(raw));
    }

    // --- Full Validate method ---

    [Fact]
    public void Validate_NullOrEmpty_ReturnsInvalid()
    {
        var result = IsbnValidator.Validate(null);
        Assert.False(result.IsValid);
        Assert.Equal("ISBN is required.", result.Error);

        result = IsbnValidator.Validate("  ");
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_ValidIsbn13_ReturnsCorrectResult()
    {
        var result = IsbnValidator.Validate("978-0-306-40615-7");
        Assert.True(result.IsValid);
        Assert.Equal("9780306406157", result.NormalizedIsbn);
        Assert.Equal("9780306406157", result.Isbn13);
        Assert.Null(result.Isbn10);
    }

    [Fact]
    public void Validate_ValidIsbn10_ConvertsToIsbn13()
    {
        var result = IsbnValidator.Validate("0-306-40615-2");
        Assert.True(result.IsValid);
        Assert.Equal("9780306406157", result.NormalizedIsbn);
        Assert.Equal("0306406152", result.Isbn10);
        Assert.Equal("9780306406157", result.Isbn13);
    }

    [Fact]
    public void Validate_InvalidIsbn_ReturnsError()
    {
        var result = IsbnValidator.Validate("123456");
        Assert.False(result.IsValid);
        Assert.Contains("not a valid ISBN", result.Error);
    }

    [Fact]
    public void Validate_Isbn10WithX_Works()
    {
        var result = IsbnValidator.Validate("080442957X");
        Assert.True(result.IsValid);
        Assert.Equal("080442957X", result.Isbn10);
        Assert.NotNull(result.Isbn13);
        Assert.Equal(result.Isbn13, result.NormalizedIsbn);
    }
}
