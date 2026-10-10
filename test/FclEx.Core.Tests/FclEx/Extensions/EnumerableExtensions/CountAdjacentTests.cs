namespace FclEx.Extensions.EnumerableExtensions;

public class CountAdjacentTests
{
    public static TheoryData<int[], (int Item, int Count)[]> Sequences => CountAdjacentTestData.Sequences;

    [Theory]
    [MemberData(nameof(Sequences))]
    public void CountsConsecutiveRuns(int[] source, (int Item, int Count)[] expected)
    {
        Assert.Equal(expected, source.CountAdjacent().ToArray());
        Assert.Equal(expected, source.CountAdjacent(null).ToArray());
    }

    [Fact]
    public void CustomComparer_RetainsFirstOriginalElementOfEachRun()
    {
        string[] source = ["a", "A", "b", "B", "A"];

        Assert.Equal([("a", 2), ("b", 2), ("A", 1)], source.CountAdjacent(StringComparer.OrdinalIgnoreCase).ToArray());
    }

    [Fact]
    public void NullElements_AreCountedNormally()
    {
        string?[] source = [null, null, "a", "a", null];

        Assert.Collection(source.CountAdjacent(),
            run =>
            {
                Assert.Null(run.Item);
                Assert.Equal(2, run.Count);
            },
            run =>
            {
                Assert.Equal("a", run.Item);
                Assert.Equal(2, run.Count);
            },
            run =>
            {
                Assert.Null(run.Item);
                Assert.Equal(1, run.Count);
            });
    }

    [Fact]
    public void NullSource_ThrowsImmediately()
    {
        IEnumerable<int> source = null!;

        Assert.Throws<ArgumentNullException>("source", () => source.CountAdjacent());
    }

    [Fact]
    public void Enumeration_IsDeferredAndReadsOnlyThroughNextRunStart()
    {
        var source = new CountAdjacentTestData.TrackingSource(1, 1, 2, 2, 3);
        var result = source.Enumerate().CountAdjacent();

        Assert.Equal(0, source.Passes);
        using (var enumerator = result.GetEnumerator())
        {
            Assert.Equal(0, source.Passes);
            Assert.True(enumerator.MoveNext());
            Assert.Equal((1, 2), enumerator.Current);
            Assert.Equal(3, source.Visits);
            Assert.True(enumerator.MoveNext());
            Assert.Equal((2, 2), enumerator.Current);
            Assert.Equal(5, source.Visits);
        }

        Assert.Equal(1, source.Disposals);
    }

    [Fact]
    public void EachEnumeration_MakesOnePassAndResetsState()
    {
        var source = new CountAdjacentTestData.TrackingSource(1, 1, 2);
        var result = source.Enumerate().CountAdjacent();

        Assert.Equal([(1, 2), (2, 1)], result.ToArray());
        Assert.Equal(1, source.Passes);
        Assert.Equal(1, source.Disposals);
        Assert.Equal([(1, 2), (2, 1)], result.ToArray());
        Assert.Equal(2, source.Passes);
        Assert.Equal(2, source.Disposals);
    }

    [Fact]
    public void ComparerException_PropagatesAndDisposesSource()
    {
        var source = new CountAdjacentTestData.TrackingSource(1, 1);
        var exception = new InvalidOperationException();
        var result = source.Enumerate().CountAdjacent(new CountAdjacentTestData.CallbackComparer(() => throw exception));

        Assert.Same(exception, Assert.Throws<InvalidOperationException>(() => result.ToArray()));
        Assert.Equal(1, source.Disposals);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CountIncrementBoundary_ReturnsMaximumOrPropagatesOverflowAndDisposesSource(bool overflow)
    {
        CountAdjacentTestData.AssertCountIncrementBoundaryAndDisposal(
            (source, comparer) => source.CountAdjacent(comparer), overflow);
    }
}
