namespace System.Collections.Generic;

public class EnumerableComparerTests
{
    [Fact]
    public void Compare_ShouldReturnZero_ForEqualSequences()
    {
        var comparer = new EnumerableComparer<int>();
        var sequence1 = new[] { 1, 2, 3 };
        var sequence2 = new[] { 1, 2, 3 };

        var result = comparer.Compare(sequence1, sequence2);

        Assert.Equal(0, result);
    }

    [Fact]
    public void Compare_ShouldReturnPositive_ForFirstSequenceGreater()
    {
        var comparer = new EnumerableComparer<int>();
        var sequence1 = new[] { 1, 2, 4 };
        var sequence2 = new[] { 1, 2, 3 };

        var result = comparer.Compare(sequence1, sequence2);

        Assert.True(result > 0);
    }

    [Fact]
    public void Compare_ShouldReturnNegative_ForFirstSequenceSmaller()
    {
        var comparer = new EnumerableComparer<int>();
        var sequence1 = new[] { 1, 2, 2 };
        var sequence2 = new[] { 1, 2, 3 };

        var result = comparer.Compare(sequence1, sequence2);

        Assert.True(result < 0);
    }

    [Fact]
    public void Compare_ShouldReturnPositive_WhenFirstSequenceIsLonger()
    {
        var comparer = new EnumerableComparer<int>();
        var sequence1 = new[] { 1, 2, 3, 4 };
        var sequence2 = new[] { 1, 2, 3 };

        var result = comparer.Compare(sequence1, sequence2);

        Assert.True(result > 0);
    }

    [Fact]
    public void Compare_ShouldReturnNegative_WhenFirstSequenceIsShorter()
    {
        var comparer = new EnumerableComparer<int>();
        var sequence1 = new[] { 1, 2 };
        var sequence2 = new[] { 1, 2, 3 };

        var result = comparer.Compare(sequence1, sequence2);

        Assert.True(result < 0);
    }

    [Fact]
    public void Compare_ShouldHandleNullSequences()
    {
        var comparer = new EnumerableComparer<int>();
        int[]? sequence1 = null;
        var sequence2 = new[] { 1, 2, 3 };

        var result1 = comparer.Compare(sequence1, sequence2);
        var result2 = comparer.Compare(sequence2, sequence1);
        var result3 = comparer.Compare(sequence1, sequence1);

        Assert.True(result1 < 0);
        Assert.True(result2 > 0);
        Assert.Equal(0, result3);
    }

    [Fact]
    public void Compare_ShouldUseCustomItemComparer()
    {
        // a reversed comparer
        var itemComparer = Comparer<int>.Create((x, y) => y.CompareTo(x));
        var comparer = new EnumerableComparer<int>(itemComparer);
        var sequence1 = new[] { 1, 2, 3 };
        var sequence2 = new[] { 3, 2, 1 };

        var result = comparer.Compare(sequence1, sequence2);

        Assert.True(result > 0);
    }
}