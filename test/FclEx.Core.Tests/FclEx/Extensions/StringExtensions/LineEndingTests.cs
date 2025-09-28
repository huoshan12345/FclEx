namespace FclEx.Extensions.StringExtensions;

public class LineEndingTests
{
    [Theory]
    [InlineData("", "")]
    [InlineData("Hello", "Hello")]
    [InlineData("Line1\nLine2", "Line1\nLine2")]
    [InlineData("Line1\r\nLine2", "Line1\nLine2")]
    [InlineData("Line1\rLine2", "Line1\nLine2")]
    [InlineData("Line1\r\nLine2\rLine3\nLine4", "Line1\nLine2\nLine3\nLine4")]
    [InlineData("Line1\n\nLine2", "Line1\n\nLine2")] // preserve blank lines
    public void LineEndingToLf_ConvertsToLf(string input, string expected)
    {
        var result = input.LineEndingToLf();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("Hello", "Hello")]
    [InlineData("Line1\nLine2", "Line1\r\nLine2")]
    [InlineData("Line1\r\nLine2", "Line1\r\nLine2")]
    [InlineData("Line1\rLine2", "Line1\r\nLine2")]
    [InlineData("Line1\r\nLine2\rLine3\nLine4", "Line1\r\nLine2\r\nLine3\r\nLine4")]
    [InlineData("Line1\n\nLine2", "Line1\r\n\r\nLine2")] // preserve blank lines
    public void LineEndingToCrLf_ConvertsToCrLf(string input, string expected)
    {
        var result = input.LineEndingToCrLf();
        Assert.Equal(expected, result);
    }

    [Fact]
    public void LineEndingToLf_IsIdempotent()
    {
        var input = "Line1\r\nLine2\r\nLine3";
        var once = input.LineEndingToLf();
        var twice = once.LineEndingToLf();
        Assert.Equal(once, twice);
    }

    [Fact]
    public void LineEndingToCrLf_IsIdempotent()
    {
        var input = "Line1\nLine2\nLine3";
        var once = input.LineEndingToCrLf();
        var twice = once.LineEndingToCrLf();
        Assert.Equal(once, twice);
    }
}
