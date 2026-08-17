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

    [Fact]
    public void AnyContainsAll_MaterializesAOneShotSourceSequence()
    {
        var enumerationCount = 0;

        IEnumerable<string> Values()
        {
            if (++enumerationCount > 1)
                throw new InvalidOperationException("The source sequence was enumerated more than once.");

            yield return "alphabet";
        }

        Assert.True(Values().AnyContainsAll(["alpha", "bet"]));
        Assert.Equal(1, enumerationCount);
    }
}
