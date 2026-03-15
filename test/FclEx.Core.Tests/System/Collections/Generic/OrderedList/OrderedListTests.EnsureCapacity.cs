// ReSharper disable GenericEnumeratorNotDisposed
// ReSharper disable UnusedVariable
// ReSharper disable CollectionNeverUpdated.Local
namespace System.Collections.Generic.OrderedList;

public abstract partial class OrderedListTests<T>
{
    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void EnsureCapacity_RequestingLargerCapacity_DoesNotInvalidateEnumeration(int count)
    {
        var list = GenericListFactory(count);
        IEnumerator<T> copiedListEnumerator = new OrderedList<T>(list).GetEnumerator();
        IEnumerator<T> enumerator = list.GetEnumerator();
        var capacity = list.Capacity;

        list.EnsureCapacity(capacity + 1);

        enumerator.MoveNext();
    }

    [Fact]
    public void EnsureCapacity_NotInitialized_RequestedZero_ReturnsZero()
    {
        var list = GenericListFactory();
        Assert.Equal(0, list.EnsureCapacity(0));
        Assert.Equal(0, list.Capacity);
    }

    [Fact]
    public void EnsureCapacity_NegativeCapacityRequested_Throws()
    {
        var list = GenericListFactory();
        AssertExtensions.Throws<ArgumentOutOfRangeException>("capacity", () => list.EnsureCapacity(-1));
    }

    public static IEnumerable<object[]> EnsureCapacity_LargeCapacity_Throws_MemberData()
    {
        yield return [5, Array.MaxLength + 1];
        yield return [1, int.MaxValue];
    }

    [Theory]
    [MemberData(nameof(EnsureCapacity_LargeCapacity_Throws_MemberData))]
    public void EnsureCapacity_LargeCapacity_Throws(int count, int requestCapacity)
    {
        var list = GenericListFactory(count);
        Assert.Throws<OutOfMemoryException>(() => list.EnsureCapacity(requestCapacity));
    }

    [Theory]
    [InlineData(5)]
    public void EnsureCapacity_RequestedCapacitySmallerThanOrEqualToCurrent_CapacityUnchanged(int currentCapacity)
    {
        var list = new OrderedList<T>(currentCapacity);

        for (var requestCapacity = 0; requestCapacity <= currentCapacity; requestCapacity++)
        {
            Assert.Equal(currentCapacity, list.EnsureCapacity(requestCapacity));
            Assert.Equal(currentCapacity, list.Capacity);
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void EnsureCapacity_RequestedCapacitySmallerThanOrEqualToCount_CapacityUnchanged(int count)
    {
        var list = GenericListFactory(count);
        var currentCapacity = list.Capacity;

        for (var requestCapacity = 0; requestCapacity <= count; requestCapacity++)
        {
            Assert.Equal(currentCapacity, list.EnsureCapacity(requestCapacity));
            Assert.Equal(currentCapacity, list.Capacity);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    public void EnsureCapacity_CapacityIsAtLeastTheRequested(int count)
    {
        var list = GenericListFactory(count);

        var currentCapacity = list.Capacity;
        var requestCapacity = currentCapacity + 1;
        var newCapacity = list.EnsureCapacity(requestCapacity);
        Assert.InRange(newCapacity, requestCapacity, int.MaxValue);
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void EnsureCapacity_RequestingLargerCapacity_DoesNotImpactListContent(int count)
    {
        var list = GenericListFactory(count);
        var copiedList = new OrderedList<T>(list);

        list.EnsureCapacity(list.Capacity + 1);
        Assert.Equal(copiedList, list);
    }
}