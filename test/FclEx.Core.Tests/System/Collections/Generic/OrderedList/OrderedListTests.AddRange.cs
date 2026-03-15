// ReSharper disable CollectionNeverUpdated.Local
// ReSharper disable PossibleMultipleEnumeration
namespace System.Collections.Generic.OrderedList;

public abstract partial class OrderedListTests<T>
{
    [Theory]
    [MemberData(nameof(EnumerableTestData))]
    public void AddRange(EnumerableType enumerableType, int listLength, int enumerableLength, int numberOfMatchingElements, int numberOfDuplicateElements)
    {
        var list = GenericListFactory(listLength);
        var sorted = list.ToList();
        var enumerable = CreateEnumerable(enumerableType, list, enumerableLength, numberOfMatchingElements, numberOfDuplicateElements);
        list.AddRange(enumerable);
        sorted.AddRange(enumerable);
        sorted.StableSort();

        Assert.Equal(listLength + enumerableLength, list.Count);
        Assert.Equal(sorted.Count, list.Count);

        // Check that the added elements are correct
        Assert.All(Enumerable.Range(0, sorted.Count), index =>
        {
            Assert.Equal(sorted[index], list[index]);
        });
    }

    [Fact]
    public void AddRange_NullList_ThrowsArgumentNullException()
    {
        AssertExtensions.Throws<ArgumentNullException>("list", () => CollectionExtensions.AddRange<int>(null!, default));
        AssertExtensions.Throws<ArgumentNullException>("list", () => CollectionExtensions.AddRange<int>(null!, new int[1]));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void AddRange_NullEnumerable_ThrowsArgumentNullException(int count)
    {
        var list = GenericListFactory(count);
        var listBeforeAdd = list.ToList();
        Assert.Throws<ArgumentNullException>(() => list.AddRange(null!));
        Assert.Equal(listBeforeAdd, list);
    }

    [Fact]
    public void AddRange_AddSelfAsEnumerable_DoesNotThrowExceptionWhenNotEmpty()
    {
        var list = GenericListFactory(0);

        // Succeeds when list is empty.
        list.AddRange(list);
        list.AddRange(list.Where(_ => true));

        // Succeeds when list has elements and is added as collection.
        list.Add(default!);
        Assert.Equal(1, list.Count);
        list.AddRange(list);
        Assert.Equal(2, list.Count);
        list.AddRange(list);
        Assert.Equal(4, list.Count);

        // does not throw because the enumerable is enumerated before adding to the list, so the list is not modified during enumeration.
        list.AddRange(list.Where(_ => true));
        Assert.Equal(8, list.Count);
        list.AddRange(list.Where(_ => true));
        Assert.Equal(16, list.Count);
    }

    [Fact]
    public void AddRange_CollectionWithLargeCount_ThrowsOutOfMemoryException()
    {
        var list = GenericListFactory(count: 1);
        var collection = new CollectionWithLargeCount<T>();

        Assert.Throws<OutOfMemoryException>(() => list.AddRange(collection));
    }
}