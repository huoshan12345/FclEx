namespace System.Collections.Generic;

public class EnumerableEqualityComparerTests
{
    [Fact]
    public void Equals_ShouldReturnTrue_ForEqualSequences()
    {
        var comparer = new EnumerableEqualityComparer<int>();
        var sequence1 = new[] { 1, 2, 3 };
        var sequence2 = new[] { 1, 2, 3 };

        var result = comparer.Equals(sequence1, sequence2);

        Assert.True(result);
    }

    [Fact]
    public void Equals_ShouldReturnFalse_ForDifferentSequences()
    {
        var comparer = new EnumerableEqualityComparer<int>();
        var sequence1 = new[] { 1, 2, 3 };
        var sequence2 = new[] { 1, 2, 4 };

        var result = comparer.Equals(sequence1, sequence2);

        Assert.False(result);
    }

    [Fact]
    public void Equals_ShouldReturnTrue_ForEmptySequences()
    {
        var comparer = new EnumerableEqualityComparer<int>();
        var sequence1 = Array.Empty<int>();
        var sequence2 = Array.Empty<int>();

        var result = comparer.Equals(sequence1, sequence2);

        Assert.True(result);
    }

    [Fact]
    public void Equals_ShouldReturnFalse_ForDifferentLengthSequences()
    {
        var comparer = new EnumerableEqualityComparer<int>();
        var sequence1 = new[] { 1, 2, 3 };
        var sequence2 = new[] { 1, 2 };

        var result = comparer.Equals(sequence1, sequence2);

        Assert.False(result);
    }

    [Fact]
    public void Equals_ShouldHandleNullSequences()
    {
        var comparer = new EnumerableEqualityComparer<int>();
        int[]? sequence1 = null;
        var sequence2 = new[] { 1, 2, 3 };

        var result1 = comparer.Equals(sequence1, sequence2);
        var result2 = comparer.Equals(sequence2, sequence1);
        var result3 = comparer.Equals(sequence1, sequence1);

        Assert.False(result1);
        Assert.False(result2);
        Assert.True(result3);
    }

    [Fact]
    public void Equals_ShouldUseCustomItemComparer()
    {
        var itemComparer = KeyEqualityComparer.Create<int, int>(m => m % 2);
        var comparer = new EnumerableEqualityComparer<int>(itemComparer);
        var sequence1 = new[] { 1, 3, 5 };
        var sequence2 = new[] { 7, 9, 11 };

        var result = comparer.Equals(sequence1, sequence2);

        Assert.True(result);
    }

    [Fact]
    public void GetHashCode_ShouldReturnSameHash_ForEqualSequences()
    {
        var comparer = new EnumerableEqualityComparer<int>();
        var sequence1 = new[] { 1, 2, 3 };
        var sequence2 = new[] { 1, 2, 3 };

        var hash1 = comparer.GetHashCode(sequence1);
        var hash2 = comparer.GetHashCode(sequence2);

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void GetHashCode_ShouldReturnDifferentHashes_ForDifferentSequences()
    {
        var comparer = new EnumerableEqualityComparer<int>();
        var sequence1 = new[] { 1, 2, 3 };
        var sequence2 = new[] { 1, 2, 4 };

        var hash1 = comparer.GetHashCode(sequence1);
        var hash2 = comparer.GetHashCode(sequence2);

        Assert.NotEqual(hash1, hash2);
    }
}