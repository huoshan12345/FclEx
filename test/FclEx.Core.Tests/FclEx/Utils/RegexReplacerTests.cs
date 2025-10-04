// ReSharper disable ConvertToConstant.Local
namespace FclEx.Utils;

public class RegexReplacerTests
{
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
}