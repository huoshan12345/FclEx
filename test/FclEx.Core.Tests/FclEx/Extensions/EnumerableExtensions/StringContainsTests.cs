namespace FclEx.Extensions.EnumerableExtensions;

public class StringContainsTests
{
    [Fact]
    public void ContainsCombinators_ShouldDescribeSubstringAndQuantifierSemantics()
    {
        var values = new[] { "alpha", "beta" };
        var patterns = new[] { "ph", "z" };

        Assert.True(values.AnyContainsAny(patterns));
        Assert.False(values.AnyContainsAll(patterns));
        Assert.False(values.AllContainsAny(patterns));
        Assert.False(values.AllContainsAll(patterns));
    }
}
