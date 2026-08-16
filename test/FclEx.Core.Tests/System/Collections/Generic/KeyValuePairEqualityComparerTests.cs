namespace System.Collections.Generic;

public class KeyValuePairEqualityComparerTests
{
    [Fact]
    public void CustomComparers_EqualPairsHaveEqualHashCodes()
    {
        var comparer = new KeyValuePairEqualityComparer<string, string>(
            StringComparer.OrdinalIgnoreCase,
            StringComparer.OrdinalIgnoreCase);
        var first = KeyValuePair.Create("KEY", "VALUE");
        var second = KeyValuePair.Create("key", "value");

        Assert.True(comparer.Equals(first, second));
        Assert.Equal(comparer.GetHashCode(first), comparer.GetHashCode(second));
        Assert.Single(new HashSet<KeyValuePair<string, string>>(comparer) { first, second });
    }
}
