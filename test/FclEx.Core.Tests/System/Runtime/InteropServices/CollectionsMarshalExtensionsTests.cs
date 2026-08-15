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
}
