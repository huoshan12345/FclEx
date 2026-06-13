namespace System.Collections.Generic;

public class NonGenericEqualityComparerAdapterTests
{
    [Fact]
    public void Equals_ShouldUseFallbackComparer_WhenComparerIsNotProvided()
    {
        var comparer = new NonGenericEqualityComparerAdapter<string>();

        Assert.True(comparer.Equals("a", "a"));
        Assert.False(comparer.Equals("a", "b"));
    }

    [Fact]
    public void Equals_ShouldHandleNullOperands_BeforeCasting()
    {
        var comparer = new NonGenericEqualityComparerAdapter<int>();

        Assert.True(comparer.Equals(null, null));
        Assert.False(comparer.Equals(null, 1));
        Assert.False(comparer.Equals(1, null));
    }

    [Fact]
    public void Equals_ShouldReturnFalse_ForDifferentOperandTypes()
    {
        var comparer = new NonGenericEqualityComparerAdapter<int>();

        Assert.False(comparer.Equals("1", 1));
    }

    [Fact]
    public void GetHashCode_ShouldUseFallbackComparer()
    {
        var comparer = new NonGenericEqualityComparerAdapter<int>();

        Assert.Equal(1.GetHashCode(), comparer.GetHashCode(1));
    }
}
