namespace FclEx.Extensions.EnumerableExtensions;

public class ContainsAnyTests
{
    [Fact]
    public void ContainsAny_WhenSequencesShareAnElement_ReturnsTrue()
    {
        Assert.True(new[] { 1, 2, 3 }.ContainsAny([4, 2]));
    }

    [Theory]
    [InlineData(new[] { 4, 5 })]
    [InlineData(new int[0])]
    public void ContainsAny_WhenSequencesDoNotShareAnElement_ReturnsFalse(int[] searchFor)
    {
        Assert.False(new[] { 1, 2, 3 }.ContainsAny(searchFor));
    }

    [Fact]
    public void ContainsAny_UsesTheSuppliedComparer()
    {
        Assert.True(new[] { "alpha" }.ContainsAny(["ALPHA"], StringComparer.OrdinalIgnoreCase));
    }

    [Fact]
    public void ContainsAny_WhenSearchForIsNull_ThrowsArgumentNullException()
    {
        IEnumerable<int>? searchFor = null;

        var exception = Assert.Throws<ArgumentNullException>(() => new[] { 1 }.ContainsAny(searchFor!));

        Assert.Equal("values", exception.ParamName);
    }
}
