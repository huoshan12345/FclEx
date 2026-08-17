namespace FclEx.Extensions;

public class RegexExtensionsTests
{
    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void TryMatch_WithAnInvalidGroupIndex_ReturnsFalse(int groupIndex)
    {
        var regex = new System.Text.RegularExpressions.Regex("(value)");

        var matched = regex.TryMatch("value", groupIndex, out var value);

        Assert.False(matched);
        Assert.Null(value);
    }
}
