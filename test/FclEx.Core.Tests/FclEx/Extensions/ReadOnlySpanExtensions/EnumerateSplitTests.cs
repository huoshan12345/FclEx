namespace FclEx.Extensions.ReadOnlySpanExtensions;

public class EnumerateSplitTests
{
    [Theory]
    [InlineData("", new[] { "" })]
    [InlineData("a,", new[] { "a", "" })]
    [InlineData(",a", new[] { "", "a" })]
    [InlineData(",,", new[] { "", "", "" })]
    public void EnumerateSplit_None_PreservesEmptyEntries(string value, string[] expected)
    {
        Assert.Equal(expected, Split(value, SplitOptions.None));
    }

    [Theory]
    [InlineData("")]
    [InlineData(",")]
    [InlineData(",,")]
    public void EnumerateSplit_RemoveEmptyEntries_RemovesEveryEmptyEntry(string value)
    {
        Assert.Empty(Split(value, SplitOptions.RemoveEmptyEntries));
    }

    [Fact]
    public void EnumerateSplit_TrimAndRemoveEmpty_AppliesBothOptions()
    {
        Assert.Equal(["a", "b"], Split(" , a, b , ", SplitOptions.TrimAndRemoveEmpty));
    }

    private static string[] Split(string value, SplitOptions options)
    {
        var result = new List<string>();
        foreach (var item in value.AsSpan().EnumerateSplit(",", options))
            result.Add(item.ToString());

        return result.ToArray();
    }
}
