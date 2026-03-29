namespace Bookly.Core.Tests;

public class MathUtilsTests
{
    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(0, 0, 0)]
    [InlineData(-1, 1, 0)]
    [InlineData(-5, -3, -8)]
    [InlineData(int.MaxValue, 0, int.MaxValue)]
    [InlineData(100, 200, 300)]
    public void Add_ReturnsCorrectSum(int a, int b, int expected)
    {
        var result = MathUtils.Add(a, b);
        Assert.Equal(expected, result);
    }
}
