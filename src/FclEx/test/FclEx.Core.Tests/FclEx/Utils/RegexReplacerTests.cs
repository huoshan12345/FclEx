// ReSharper disable ConvertToConstant.Local
namespace FclEx.Utils;

public class RegexReplacerTests
{
    [Fact]
    public void Replace_LF_TO_CRLF_ShouldReplaceLineFeed()
    {
        var replacer = RegexReplacer.LF_TO_CRLF;
        var input = "Line 1\nLine 2\nLine 3";
        var expected = "Line 1\r\nLine 2\r\nLine 3";
        var result = replacer.Replace(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Replace_CRLF_TO_LF_ShouldReplaceCarriageReturnLineFeed()
    {
        var replacer = RegexReplacer.CRLF_TO_LF;
        var input = "Line 1\r\nLine 2\r\nLine 3";
        var expected = "Line 1\nLine 2\nLine 3";
        var result = replacer.Replace(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Replace_CustomPattern_ShouldReplaceUsingCustomPattern()
    {
        var pattern = @"\d+";
        var replacement = "NUM";
        var replacer = new RegexReplacer(pattern, replacement);
        var input = "There are 123 apples and 456 oranges.";
        var expected = "There are NUM apples and NUM oranges.";
        var result = replacer.Replace(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Replace_EmptyInput_ShouldReturnEmptyString()
    {
        var replacer = RegexReplacer.LF_TO_CRLF;
        var input = string.Empty;
        var expected = string.Empty;
        var result = replacer.Replace(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Replace_NoMatches_ShouldReturnOriginalString()
    {
        var replacer = RegexReplacer.LF_TO_CRLF;
        var input = "This string has no line feeds.";
        var result = replacer.Replace(input);
        Assert.Equal(input, result);
    }
}