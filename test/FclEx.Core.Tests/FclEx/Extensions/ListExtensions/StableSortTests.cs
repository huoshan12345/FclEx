namespace FclEx.Extensions;

public class StableSortTests
{
    private sealed record Item(int Key, string Id);

    [Fact]
    public void StableSort_ShouldSortOnlyTheRequestedRangeAndPreserveEqualOrder()
    {
        var comparer = Comparer<Item>.Create((x, y) => x.Key.CompareTo(y.Key));
        IList<Item> items =
        [
            new(9, "prefix"),
            new(2, "first-2"),
            new(1, "only-1"),
            new(2, "second-2"),
            new(8, "suffix"),
        ];

        items.StableSort(1, 3, comparer);

        Assert.Equal(
            ["prefix", "only-1", "first-2", "second-2", "suffix"],
            items.Select(x => x.Id));
    }

    [Fact]
    public void StableSort_ShouldValidateRangesEvenForAnEmptyList()
    {
        IList<int> items = [];

        items.StableSort(0, 0);
        Assert.Throws<ArgumentOutOfRangeException>(() => items.StableSort(1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => items.StableSort(0, -1));
        Assert.Throws<ArgumentException>(() => new List<int> { 1, 2 }.StableSort(1, 2));
    }

    [Fact]
    public void StableSort_ShouldNotModifyTheRangeWhenTheComparerThrows()
    {
        IList<int> items = [9, 3, 2, 1, 8];
        var comparer = Comparer<int>.Create((_, _) => throw new InvalidOperationException());

        Assert.Throws<InvalidOperationException>(() => items.StableSort(1, 3, comparer));

        Assert.Equal([9, 3, 2, 1, 8], items);
    }
}
