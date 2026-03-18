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
    public void AddRange_AddSelfAsEnumerable_ThrowsExceptionWhenNotEmpty()
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

        // Fails version check when list has elements and is added as non-collection.
        Assert.Throws<InvalidOperationException>(() => list.AddRange(list.Where(_ => true)));
        Assert.Equal(5, list.Count);
        Assert.Throws<InvalidOperationException>(() => list.AddRange(list.Where(_ => true)));
        Assert.Equal(6, list.Count);
    }

    [Fact]
    public void AddRange_CollectionWithLargeCount_ThrowsOverflowException()
    {
        var list = GenericListFactory(count: 1);
        ICollection<T> collection = new CollectionWithLargeCount();

        Assert.Throws<OverflowException>(() => list.AddRange(collection));
    }

    private class CollectionWithLargeCount : ICollection<T>
    {
        public int Count => int.MaxValue;

        public bool IsReadOnly => throw new NotImplementedException();
        public void Add(T item) => throw new NotImplementedException();
        public void Clear() => throw new NotImplementedException();
        public bool Contains(T item) => throw new NotImplementedException();
        public void CopyTo(T[] array, int arrayIndex) => throw new NotImplementedException();
        public IEnumerator<T> GetEnumerator() => throw new NotImplementedException();
        public bool Remove(T item) => throw new NotImplementedException();
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
    }
}