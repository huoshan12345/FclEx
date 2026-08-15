namespace System.Runtime.InteropServices;

public class CollectionsMarshalExtensionsTests
{
    [Fact]
    public void ArrayBasedCollection_ShouldExposeReadOnlyElements()
    {
        var collection = new OrderedList<int>([3, 1, 2]);

        Assert.Equal([1, 2, 3], collection.AsReadOnlySpan().ToArray());
    }

    [Fact]
    public void AsSpan_ShouldExposeWritableStorageThroughCollectionsMarshal()
    {
        var collection = new OrderedList<int>([3, 1, 2]);

        var span = CollectionsMarshal.AsSpan(collection);
        span[0] = 10;

        Assert.Equal(10, collection[0]);
    }

    [Fact]
    public void AsSpan_ShouldReturnEmptyForNullCollection()
    {
        OrderedList<int>? collection = null;

        Assert.True(CollectionsMarshal.AsSpan(collection).IsEmpty);
    }

    [Fact]
    public void Items_ShouldExposeTheCompleteListCapacityArray()
    {
        var list = new List<int>(4) { 1 };

        var items = CollectionsMarshal.Items(list);
        items[0] = 2;

        Assert.Equal(list.Capacity, items.Length);
        Assert.Equal(2, list[0]);
    }

    [Fact]
    public void SetCount_ShouldExposeInitializedCapacitySlots()
    {
        var list = new List<int>(4) { 1 };
        var items = CollectionsMarshal.Items(list);
        items[1] = 2;

        CollectionsMarshal.SetCount(list, 2);

        Assert.Equal([1, 2], list);
    }
}
