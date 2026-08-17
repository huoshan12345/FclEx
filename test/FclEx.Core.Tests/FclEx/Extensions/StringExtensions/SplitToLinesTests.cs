namespace FclEx.Extensions.StringExtensions;

public class SplitToLinesTests
{
    [Theory]
    [InlineData("one\r\ntwo", "one", "two")]
    [InlineData("one\rtwo", "one", "two")]
    [InlineData("one\ntwo", "one", "two")]
    [InlineData("one\r\ntwo\rthree\nfour", "one", "two", "three", "four")]
    public void Splits_Each_NewLine_Sequence_Once(string input, params string[] expected)
    {
        Assert.Equal(expected, input.SplitToLines(SplitOptions.None));
    }

    [Fact]
    public void None_Preserves_Empty_Lines()
    {
        Assert.Equal(["one", "", "two"], "one\r\n\r\ntwo".SplitToLines(SplitOptions.None));
        Assert.Equal(["one", "", "two"], "one\r\n\r\ntwo".SplitToLines(StringSplitOptions.None));
    }

    [Fact]
    public void TrimAndRemoveEmpty_Trims_Lines_Before_Removing_Empty_Ones()
    {
        Assert.Equal(["one", "two"], " one \r\n  \r\n two ".SplitToLines());
    }
}
