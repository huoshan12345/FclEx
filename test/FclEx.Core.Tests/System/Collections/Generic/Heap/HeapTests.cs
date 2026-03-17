#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
#pragma warning disable IDE0060 // Remove unused parameter
namespace System.Collections.Generic.Heap;

public abstract class HeapTests<T> : IGenericSharedAPI_Tests<T>
{
    //protected override bool EnumeratorCurrentUndefinedOperationThrows => false;

    #region Heap<T> Helper Methods

    #region IGenericSharedAPI<T> Helper Methods

    protected Heap<T> HeapFactory() => [];

    protected Heap<T> HeapFactory(int count)
    {
        var heap = new Heap<T>();
        var seed = count * 34;
        for (var i = 0; i < count; i++)
            heap.Push(CreateT(seed++));
        return heap;
    }

    #endregion

    protected override IEnumerable<T> GenericIEnumerableFactory()
    {
        return HeapFactory();
    }

    protected override IEnumerable<T> GenericIEnumerableFactory(int count)
    {
        return HeapFactory(count);
    }

    protected override int Count(IEnumerable<T> enumerable) { return ((Heap<T>)enumerable).Count; }
    protected override void Add(IEnumerable<T> enumerable, T value) { ((Heap<T>)enumerable).Push(value); }
    protected override void Clear(IEnumerable<T> enumerable) { ((Heap<T>)enumerable).Clear(); }
    protected override bool Contains(IEnumerable<T> enumerable, T value) { return ((Heap<T>)enumerable).Contains(value); }
    protected override void CopyTo(IEnumerable<T> enumerable, T[] array, int index) { ((Heap<T>)enumerable).CopyTo(array, index); }
    protected override bool Remove(IEnumerable<T> enumerable) { ((Heap<T>)enumerable).Pop(); return true; }

    #endregion

    #region Constructor

    #endregion

    #region Constructor_IEnumerable

    [Theory]
    [MemberData(nameof(EnumerableTestData))]
    public void Generic_Constructor_IEnumerable(EnumerableType enumerableType, int setLength, int enumerableLength, int numberOfMatchingElements, int numberOfDuplicateElements)
    {
        var arr = CreateEnumerable(enumerableType, null!, enumerableLength, 0, numberOfDuplicateElements).ToArray();
        var heap = new Heap<T>(arr, Comparer<T>.Default);
        Assert.Equal(arr.Length, heap.Count);
        Array.Sort(arr, Comparer<T>.Default);
        foreach (var item in arr)
        {
            Assert.Equal(item, heap.Pop());
        }
    }

    [Fact]
    public void Generic_Constructor_IEnumerable_Null_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>("source", () => new Heap<T>((IEnumerable<T>)null!));
    }

    #endregion

    #region Pop

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void Heap_Generic_Pop_AllElements(int count)
    {
        var heap = HeapFactory(count);
        var elements = heap.ToList();
        elements.Sort();
        foreach (var element in elements)
            Assert.Equal(element, heap.Pop());
    }

    [Fact]
    public void Heap_Generic_Pop_OnEmptyHeap_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => new Heap<T>().Pop());
    }

    #endregion

    #region ToArray

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void Heap_Generic_ToArray(int count)
    {
        var heap = HeapFactory(count);
        Assert.True(ArrayExtensions.SequenceEqual(heap.ToArray(), heap.ToArray<T>()));
    }

    #endregion

    #region Peek

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void Heap_Generic_Peek_AllElements(int count)
    {
        var heap = HeapFactory(count);
        var elements = heap.ToList();
        elements.Sort();
        foreach (var element in elements)
        {
            Assert.Equal(element, heap.Peek());
            heap.Pop();
        }
    }

    [Fact]
    public void Heap_Generic_Peek_OnEmptyHeap_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => new Heap<T>().Peek());
    }

    #endregion
}