namespace FclEx.Extensions.EnumerableExtensions;

public class DistinctAdjacentByTests
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
    public void IdentitySelector_MatchesAdjacentDistinct(int[] source, int[] expected)
    {
        Assert.Equal(expected, source.DistinctAdjacentBy(item => item).ToArray());
        Assert.Equal(expected, source.DistinctAdjacentBy(item => item, null).ToArray());
    }

    [Fact]
    public void EqualKeys_RetainFirstSourceElementOfEachRun()
    {
        var source = new[] { "one", "two", "four", "five", "six" };

        Assert.Equal(["one", "four", "six"], source.DistinctAdjacentBy(item => item.Length).ToArray());
    }

    [Fact]
    public void CustomKeyComparer_RetainsFirstSourceElementOfEachRun()
    {
        var source = new[] { (Key: "a", Id: 1), (Key: "A", Id: 2), (Key: "b", Id: 3), (Key: "a", Id: 4) };

        var result = source.DistinctAdjacentBy(item => item.Key, StringComparer.OrdinalIgnoreCase).ToArray();

        Assert.Equal([source[0], source[2], source[3]], result);
    }

    [Fact]
    public void NullElementsAndKeys_AreSupported()
    {
        string?[] source = [null, null, "a", "b", null, null];

        Assert.Collection(source.DistinctAdjacentBy(item => item?.Length),
            item => Assert.Null(item),
            item => Assert.Equal("a", item),
            item => Assert.Null(item));
    }

    [Fact]
    public void NullSource_ThrowsImmediately()
    {
        IEnumerable<int> source = null!;

        Assert.Throws<ArgumentNullException>("source", () => source.DistinctAdjacentBy(item => item));
    }

    [Fact]
    public void NullSelector_ThrowsImmediatelyEvenForEmptySource()
    {
        Assert.Throws<ArgumentNullException>("keySelector", () => Array.Empty<int>().DistinctAdjacentBy<int, int>(null!));
    }

    [Fact]
    public void EnumerationAndKeySelection_AreDeferredAndDoNotReadAhead()
    {
        var visited = 0;
        var selected = 0;
        IEnumerable<int> Source()
        {
            while (true)
            {
                visited++;
                yield return visited;
            }
        }

        var result = Source().DistinctAdjacentBy(item =>
        {
            selected++;
            return item;
        });

        Assert.Equal(0, visited);
        Assert.Equal(0, selected);
        Assert.Equal([1, 2, 3], result.Take(3).ToArray());
        Assert.Equal(3, visited);
        Assert.Equal(3, selected);
    }

    [Fact]
    public void EachEnumeration_SelectsEachKeyOnceAndUsesIndependentState()
    {
        var passes = 0;
        var selected = new List<int>();
        IEnumerable<int> Source()
        {
            passes++;
            yield return 1;
            yield return 1;
            yield return 2;
        }

        var result = Source().DistinctAdjacentBy(item =>
        {
            selected.Add(item);
            return item;
        });

        Assert.Equal([1, 2], result.ToArray());
        Assert.Equal(1, passes);
        Assert.Equal([1, 1, 2], selected);
        selected.Clear();
        Assert.Equal([1, 2], result.ToArray());
        Assert.Equal(2, passes);
        Assert.Equal([1, 1, 2], selected);
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

        var result = Source().DistinctAdjacentBy(item => item);
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

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void SelectorException_PropagatesAndDisposesSource(int failingItem)
    {
        var disposed = false;
        var exception = new InvalidOperationException();
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

        var result = Source().DistinctAdjacentBy(item => item == failingItem ? throw exception : item);

        Assert.Same(exception, Assert.Throws<InvalidOperationException>(() => result.ToArray()));
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

        var result = Source().DistinctAdjacentBy(item => item, new ThrowingComparer());

        Assert.Throws<InvalidOperationException>(() => result.ToArray());
        Assert.True(disposed);
    }

    private sealed class ThrowingComparer : IEqualityComparer<int>
    {
        public bool Equals(int first, int second) => throw new InvalidOperationException();

        public int GetHashCode(int value) => throw new NotSupportedException();
    }
}
