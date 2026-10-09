namespace FclEx.Extensions.EnumerableExtensions;

public class CountAdjacentByTests
{
    public static TheoryData<int[], (int Key, int Count)[]> Sequences => CountAdjacentTestData.Sequences;

    [Theory]
    [MemberData(nameof(Sequences))]
    public void IdentitySelector_CountsConsecutiveRuns(int[] source, (int Key, int Count)[] expected)
    {
        Assert.Equal(expected, source.CountAdjacentBy(item => item).ToArray());
        Assert.Equal(expected, source.CountAdjacentBy(item => item, null).ToArray());
    }

    [Fact]
    public void KeySelector_ReturnsKeysRatherThanOriginalElements()
    {
        string[] source = ["one", "two", "four", "five", "six"];

        Assert.Equal([(3, 2), (4, 2), (3, 1)], source.CountAdjacentBy(item => item.Length).ToArray());
    }

    [Fact]
    public void CustomComparer_RetainsFirstSelectedKeyOfEachRun()
    {
        var source = new[] { (Key: "a", Id: 1), (Key: "A", Id: 2), (Key: "b", Id: 3), (Key: "A", Id: 4) };
        var result = source.CountAdjacentBy(item => item.Key, StringComparer.OrdinalIgnoreCase);

        Assert.Equal([("a", 2), ("b", 1), ("A", 1)], result.ToArray());
    }

    [Fact]
    public void NullElementsAndKeys_AreCountedNormally()
    {
        string?[] source = [null, null, "a", "b", null];

        Assert.Equal([(null, 2), ((int?)1, 2), (null, 1)], source.CountAdjacentBy(item => item?.Length).ToArray());
    }

    [Fact]
    public void NullSource_ThrowsImmediately()
    {
        IEnumerable<int> source = null!;

        Assert.Throws<ArgumentNullException>("source", () => source.CountAdjacentBy(item => item));
    }

    [Fact]
    public void NullSelector_ThrowsImmediatelyEvenForEmptySource()
    {
        Assert.Throws<ArgumentNullException>("keySelector", () => Array.Empty<int>().CountAdjacentBy<int, int>(null!));
    }

    [Fact]
    public void EmptySource_DoesNotInvokeSelector()
    {
        Assert.Empty(Array.Empty<int>().CountAdjacentBy<int, int>(_ => throw new InvalidOperationException()));
    }

    [Fact]
    public void EnumerationAndKeySelection_AreDeferredAndReadOnlyThroughNextRunStart()
    {
        var source = new CountAdjacentTestData.TrackingSource(1, 1, 2, 2, 3);
        var selected = new List<int>();
        var result = source.Enumerate().CountAdjacentBy(item =>
        {
            selected.Add(item);
            return item;
        });

        Assert.Equal(0, source.Passes);
        Assert.Empty(selected);
        using (var enumerator = result.GetEnumerator())
        {
            Assert.Equal(0, source.Passes);
            Assert.Empty(selected);
            Assert.True(enumerator.MoveNext());
            Assert.Equal((1, 2), enumerator.Current);
            Assert.Equal([1, 1, 2], selected);
            Assert.True(enumerator.MoveNext());
            Assert.Equal((2, 2), enumerator.Current);
            Assert.Equal([1, 1, 2, 2, 3], selected);
            Assert.Equal(5, source.Visits);
        }

        Assert.Equal(1, source.Disposals);
    }

    [Fact]
    public void EachEnumeration_SelectsEachKeyOnceAndResetsState()
    {
        var source = new CountAdjacentTestData.TrackingSource(1, 1, 2);
        var selections = 0;
        var result = source.Enumerate().CountAdjacentBy(item =>
        {
            selections++;
            return item;
        });

        Assert.Equal([(1, 2), (2, 1)], result.ToArray());
        Assert.Equal(1, source.Passes);
        Assert.Equal(1, source.Disposals);
        Assert.Equal(3, selections);
        Assert.Equal([(1, 2), (2, 1)], result.ToArray());
        Assert.Equal(2, source.Passes);
        Assert.Equal(2, source.Disposals);
        Assert.Equal(6, selections);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void SelectorException_PropagatesAndDisposesSource(int failingItem)
    {
        var source = new CountAdjacentTestData.TrackingSource(1, 2);
        var exception = new InvalidOperationException();
        var result = source.Enumerate().CountAdjacentBy(item => item == failingItem ? throw exception : item);

        Assert.Same(exception, Assert.Throws<InvalidOperationException>(() => result.ToArray()));
        Assert.Equal(1, source.Disposals);
    }

    [Fact]
    public void ComparerException_PropagatesAndDisposesSource()
    {
        var source = new CountAdjacentTestData.TrackingSource(1, 1);
        var exception = new InvalidOperationException();
        var result = source.Enumerate().CountAdjacentBy(item => item,
            new CountAdjacentTestData.CallbackComparer(() => throw exception));

        Assert.Same(exception, Assert.Throws<InvalidOperationException>(() => result.ToArray()));
        Assert.Equal(1, source.Disposals);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CountBoundary_ReturnsMaximumOrThrowsOnOverflowAndDisposesSource(bool overflow)
    {
        CountAdjacentTestData.AssertCountBoundaryWithInjectedCounter(
            (source, comparer) => source.CountAdjacentBy(item => item, comparer), overflow);
    }
}
