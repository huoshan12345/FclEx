namespace FclEx.Extensions.EnumerableExtensions;

public class DistinctAdjacentTests
{
    public static TheoryData<int[], int[]> Sequences => new()
    {
        { [], [] },
        { [1], [1] },
        { [1, 1, 1], [1] },
        { [1, 2, 3], [1, 2, 3] },
        { [1, 1, 2, 2, 2, 1], [1, 2, 1] },
        { [0, 0, -1, -1, 0], [0, -1, 0] }
    };

    [Theory]
    [MemberData(nameof(Sequences))]
    public void RemovesOnlyConsecutiveDuplicates(int[] source, int[] expected)
    {
        Assert.Equal(expected, source.DistinctAdjacent().ToArray());
        Assert.Equal(expected, source.DistinctAdjacent(null).ToArray());
    }

    [Fact]
    public void CustomComparer_RetainsFirstElementOfEachRun()
    {
        var source = new[] { "a", "A", "b", "B", "a" };

        Assert.Equal(["a", "b", "a"], source.DistinctAdjacent(StringComparer.OrdinalIgnoreCase).ToArray());
    }

    [Fact]
    public void NullElements_AreComparedNormally()
    {
        string?[] source = [null, null, "a", "a", null, null];

        Assert.Collection(source.DistinctAdjacent(),
            item => Assert.Null(item),
            item => Assert.Equal("a", item),
            item => Assert.Null(item));
    }

    [Fact]
    public void NullSource_ThrowsImmediately()
    {
        IEnumerable<int> source = null!;

        Assert.Throws<ArgumentNullException>("source", () => source.DistinctAdjacent());
    }

    [Fact]
    public void Enumeration_IsDeferredAndDoesNotReadAhead()
    {
        var visited = 0;
        IEnumerable<int> Source()
        {
            while (true)
            {
                visited++;
                yield return visited;
            }
        }

        var result = Source().DistinctAdjacent();
        Assert.Equal(0, visited);
        Assert.Equal([1, 2, 3], result.Take(3).ToArray());
        Assert.Equal(3, visited);
    }

    [Fact]
    public void EachEnumeration_UsesOneSourcePassAndIndependentState()
    {
        var passes = 0;
        IEnumerable<int> Source()
        {
            passes++;
            yield return 1;
            yield return 1;
            yield return 2;
        }

        var result = Source().DistinctAdjacent();
        Assert.Equal([1, 2], result.ToArray());
        Assert.Equal(1, passes);
        Assert.Equal([1, 2], result.ToArray());
        Assert.Equal(2, passes);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SourceEnumerator_IsDisposedOnCompletionOrEarlyStop(bool stopEarly)
    {
        var disposed = false;
        IEnumerable<int> Source()
        {
            try
            {
                yield return 1;
                yield return 2;
            }
            finally
            {
                disposed = true;
            }
        }

        var result = Source().DistinctAdjacent();
        if (stopEarly)
        {
            Assert.Equal(1, result.First());
        }
        else
        {
            Assert.Equal([1, 2], result.ToArray());
        }

        Assert.True(disposed);
    }

    [Fact]
    public void ComparerException_PropagatesAndDisposesSource()
    {
        var disposed = false;
        IEnumerable<int> Source()
        {
            try
            {
                yield return 1;
                yield return 2;
            }
            finally
            {
                disposed = true;
            }
        }

        var result = Source().DistinctAdjacent(new ThrowingComparer());
        Assert.Throws<InvalidOperationException>(() => result.ToArray());
        Assert.True(disposed);
    }

    private sealed class ThrowingComparer : IEqualityComparer<int>
    {
        public bool Equals(int first, int second) => throw new InvalidOperationException();

        public int GetHashCode(int value) => throw new NotSupportedException();
    }
}
