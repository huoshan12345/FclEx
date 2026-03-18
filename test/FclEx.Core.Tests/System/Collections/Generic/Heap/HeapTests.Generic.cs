// ReSharper disable CollectionNeverUpdated.Local
namespace System.Collections.Generic.Heap;

public abstract class HeapTests<T> : IGenericSharedAPI_Tests<T>
{
    #region PriorityQueue Helper methods
    protected virtual IComparer<T>? GetComparer() => Comparer<T>.Default;

    protected IEnumerable<T> CreateItems(int count)
    {
        const int magicValue = 34;
        var seed = count * magicValue;
        for (var i = 0; i < count; i++)
        {
            yield return CreateT(seed++);
        }
    }

    protected Heap<T> CreateEmptyHeap(int initialCapacity = 0)
        => new(initialCapacity, GetComparer());

    protected Heap<T> CreateHeap(int initialCapacity, int countOfItemsToGenerate, out List<T> generatedItems)
    {
        generatedItems = CreateItems(countOfItemsToGenerate).ToList();
        var heap = new Heap<T>(initialCapacity, GetComparer());
        heap.PushRange(generatedItems);
        return heap;
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void CreateWithCapacity_EqualsCapacityProperty(int capacity)
    {
        var heap = new Heap<T>(capacity);
        Assert.Equal(capacity, heap.Capacity);
    }

    [Fact]
    public void Heap_EnsureCapacityThenTrimExcess_CapacityUpdates()
    {
        var heap = new Heap<T>(2);
        Assert.Equal(2, heap.Capacity);

        heap.EnsureCapacity(12);
        Assert.InRange(heap.Capacity, 12, int.MaxValue);

        heap.TrimExcess();
        Assert.Equal(0, heap.Capacity);
    }

    #endregion

    #region Constructors

    [Fact]
    public void Heap_DefaultConstructor_ComparerEqualsDefaultComparer()
    {
        var heap = new Heap<T>();

        Assert.Equal(expected: 0, heap.Count);
        Assert.Empty(heap);
        Assert.Equal(heap.Comparer, Comparer<T>.Default);
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void Heap_EmptyCollection_UnorderedItemsIsEmpty(int initialCapacity)
    {
        var heap = new Heap<T>(initialCapacity);
        Assert.Empty(heap);
    }

    [Fact]
    public void Heap_ComparerConstructor_ComparerShouldEqualParameter()
    {
        var comparer = GetComparer();
        var queue = new Heap<T>(comparer);
        Assert.Equal(comparer, queue.Comparer);
    }

    [Fact]
    public void Heap_ComparerConstructorNull_ComparerShouldEqualDefaultComparer()
    {
        var queue = new Heap<T>(comparer: null);
        Assert.Equal(0, queue.Count);
        Assert.Same(Comparer<T>.Default, queue.Comparer);
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void Heap_CapacityConstructor_ComparerShouldEqualDefaultComparer(int initialCapacity)
    {
        var heap = new Heap<T>(initialCapacity);
        Assert.Empty(heap);
        Assert.Same(Comparer<T>.Default, heap.Comparer);
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void Heap_EnumerableConstructor_ShouldContainAllElements(int count)
    {
        var itemsToEnqueue = CreateItems(count).ToArray();
        var heap = new Heap<T>(itemsToEnqueue, GetComparer());
        Assert.Equal(itemsToEnqueue.Length, heap.Count);
        AssertExtensions.CollectionEqual(itemsToEnqueue, heap, EqualityComparer<T>.Default);
    }

    #endregion

    #region Push, Pop, Peek, PushPop, PopPush

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void Heap_Enqueue_IEnumerable(int count)
    {
        var itemsToEnqueue = CreateItems(count).ToArray();
        var queue = CreateEmptyHeap();

        foreach (var item in itemsToEnqueue)
        {
            queue.Push(item);
        }

        AssertExtensions.CollectionEqual(itemsToEnqueue, queue, EqualityComparer<T>.Default);
    }

    [Theory]
    [MemberData(nameof(ValidPositiveCollectionSizes))]
    public void Heap_Peek_ShouldReturnMinimalElement(int count)
    {
        IReadOnlyCollection<T> itemsToEnqueue = CreateItems(count).ToArray();
        var heap = CreateEmptyHeap();
        var minItem = itemsToEnqueue.First();

        foreach (var item in itemsToEnqueue)
        {
            if (heap.Comparer.Compare(item, minItem) < 0)
            {
                minItem = item;
            }

            heap.Push(item);

            var actualPeekElement = heap.Peek();
            Assert.Equal(minItem, actualPeekElement);

            var actualTryPeekSuccess = heap.TryPeek(out var actualTryPeekElement);
            Assert.True(actualTryPeekSuccess);
            Assert.Equal(minItem, actualTryPeekElement);
        }
    }

    [Theory]
    [InlineData(0, 5)]
    [InlineData(1, 1)]
    [InlineData(3, 100)]
    public void Heap_PeekAndDequeue(int initialCapacity, int count)
    {
        var queue = CreateHeap(initialCapacity, count, out List<T> generatedItems);

        var expectedPeekPriorities = generatedItems
            .OrderBy(x => x, queue.Comparer)
            .ToArray();

        for (var i = 0; i < count; ++i)
        {
            var expected = expectedPeekPriorities[i];

            var actualTryPeekSuccess = queue.TryPeek(out var actualTryPeekElement);
            var actualTryDequeueSuccess = queue.TryPop(out var actualTryDequeueElement);

            Assert.True(actualTryPeekSuccess);
            Assert.True(actualTryDequeueSuccess);
            Assert.Equal(expected, actualTryPeekElement);
            Assert.Equal(expected, actualTryDequeueElement);
        }

        Assert.Equal(expected: 0, queue.Count);
        Assert.False(queue.TryPeek(out _));
        Assert.False(queue.TryPop(out _));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void Heap_PushRange_IEnumerable(int count)
    {
        var itemsToEnqueue = CreateItems(count).ToArray();
        var queue = CreateEmptyHeap();

        queue.PushRange(itemsToEnqueue);

        AssertExtensions.CollectionEqual(itemsToEnqueue, queue, EqualityComparer<T>.Default);
    }

    [Fact]
    public void Heap_PushRange_CollectionWithLargeCount_ThrowsOverflowException()
    {
        var queue = CreateHeap(1, 1, out _);

        CollectionWithLargeCount<T> pairCollection = [];
        Assert.Throws<OverflowException>(() => queue.PushRange(pairCollection));

        CollectionWithLargeCount<T> elementCollection = [];
        Assert.Throws<OverflowException>(() => queue.PushRange(elementCollection));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void Heap_PushPop(int count)
    {
        var itemsToEnqueue = CreateItems(2 * count).ToArray();
        var queue = CreateEmptyHeap();
        queue.PushRange(itemsToEnqueue.Take(count));

        foreach (var item in itemsToEnqueue.Skip(count))
        {
            queue.PushPop(item);
        }

        var expectedItems = itemsToEnqueue.OrderByDescending(x => x, queue.Comparer).Take(count);
        AssertExtensions.CollectionEqual(expectedItems, queue, EqualityComparer<T>.Default);
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void Heap_DequeueEnqueue(int count)
    {
        var itemsToEnqueue = CreateItems(count * 2).ToArray();
        var queue = CreateEmptyHeap();
        queue.PushRange(itemsToEnqueue.Take(count));

        var dequeuedItems = new List<T>();
        foreach (var item in itemsToEnqueue.Skip(count))
        {
            queue.TryPeek(out var dequeuedElement);
            dequeuedItems.Add(dequeuedElement);
            queue.PopPush(item);
        }

        Assert.Equal(dequeuedItems.Count, count);

        var expectedItems = itemsToEnqueue.Where(item => !dequeuedItems.Contains(item, EqualityComparer<T>.Default));
        AssertExtensions.CollectionEqual(expectedItems, queue, EqualityComparer<T>.Default);
    }

    #endregion

    #region Clear

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void Heap_Clear(int count)
    {
        var queue = CreateHeap(initialCapacity: 0, count, out _);
        Assert.Equal(count, queue.Count);

        queue.Clear();

        Assert.Equal(expected: 0, queue.Count);
        Assert.False(queue.TryPeek(out _));
    }

    #endregion

    #region Enumeration

    [Theory]
    [MemberData(nameof(ValidPositiveCollectionSizes))]
    public void Heap_Enumeration_OrderingIsConsistent(int count)
    {
        var queue = CreateHeap(initialCapacity: 0, count, out _);

        var firstEnumeration = queue.ToArray();
        var secondEnumeration = queue.ToArray();

        Assert.Equal(firstEnumeration.Length, count);
        Assert.True(firstEnumeration.SequenceEqual(secondEnumeration));
    }

    #endregion

    #region IGenericSharedAPI<T> Helper Methods

    /// <summary>
    /// <see cref="IGenericSharedAPI_Tests{T}"/> requires collections that implement IEnumerable.
    /// Since PriorityQueue does not we use a subclass that delegates to <see cref="PriorityQueue{TElement, TPriority}.UnorderedItems"/>.
    /// </summary>
    protected class EnumerableHeap : Heap<T>, IEnumerable<T>
    {
        public EnumerableHeap(IComparer<T>? comparer) : base(comparer)
        {
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    protected override IEnumerable<T> GenericIEnumerableFactory() => new EnumerableHeap(GetComparer());
    protected override int Count(IEnumerable<T> enumerable) => ((EnumerableHeap)enumerable).Count;
    protected override void Add(IEnumerable<T> enumerable, T value) => ((EnumerableHeap)enumerable).Push(value);
    protected override void Clear(IEnumerable<T> enumerable) => ((EnumerableHeap)enumerable).Clear();
    protected override bool Contains(IEnumerable<T> enumerable, T value) => ((EnumerableHeap)enumerable).Any(elem => ((Heap<T>)enumerable).Comparer.Compare(elem, value) == 0);
    protected override void CopyTo(IEnumerable<T> enumerable, T[] array, int index) => ((ICollection<T>)(EnumerableHeap)enumerable).CopyTo(array, index);
    protected override bool Remove(IEnumerable<T> enumerable) => ((EnumerableHeap)enumerable).TryPop(out _);
    protected override Type IGenericSharedAPI_CopyTo_IndexLargerThanArrayCount_ThrowType => typeof(ArgumentException);

    #endregion
}