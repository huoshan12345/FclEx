namespace System.Collections.Generic;

public class HeapExceptionSafetyTests
{
    private sealed class ComparerException : Exception { }

    private sealed class ThrowingComparer : IComparer<int>
    {
        private int _comparisonCount;
        private int? _throwOnComparison;

        public int Compare(int x, int y)
        {
            _comparisonCount++;
            if (_comparisonCount == _throwOnComparison)
                throw new ComparerException();

            return x.CompareTo(y);
        }

        public void ThrowOnComparison(int comparison)
        {
            _comparisonCount = 0;
            _throwOnComparison = comparison;
        }

        public void StopThrowing()
        {
            _comparisonCount = 0;
            _throwOnComparison = null;
        }
    }

    [Fact]
    public void Push_ShouldLeaveHeapAndCapacityUnchangedWhenComparerThrows()
    {
        var comparer = new ThrowingComparer();
        var heap = CreateHeap(comparer);
        var capacity = heap.Capacity;

        AssertOperationIsAtomic(heap, comparer, 1, () => heap.Push(0));

        Assert.Equal(capacity, heap.Capacity);
    }

    [Fact]
    public void Pop_ShouldLeaveHeapUnchangedWhenComparerThrows()
    {
        var comparer = new ThrowingComparer();
        var heap = CreateHeap(comparer);

        AssertOperationIsAtomic(heap, comparer, 1, () => heap.Pop());
    }

    [Fact]
    public void PopPush_ShouldLeaveHeapUnchangedWhenComparerThrows()
    {
        var comparer = new ThrowingComparer();
        var heap = CreateHeap(comparer);

        AssertOperationIsAtomic(heap, comparer, 1, () => heap.PopPush(10));
    }

    [Fact]
    public void PushPop_ShouldLeaveHeapUnchangedWhenLaterComparisonThrows()
    {
        var comparer = new ThrowingComparer();
        var heap = CreateHeap(comparer);

        AssertOperationIsAtomic(heap, comparer, 2, () => heap.PushPop(10));
    }

    private static Heap<int> CreateHeap(ThrowingComparer comparer)
    {
        return new Heap<int>([1, 3, 2, 7, 6, 5, 4], comparer);
    }

    private static void AssertOperationIsAtomic(
        Heap<int> heap,
        ThrowingComparer comparer,
        int throwOnComparison,
        Action operation)
    {
        var items = heap.ToArray();
        var count = heap.Count;
        var enumerator = heap.GetEnumerator();
        Assert.True(enumerator.MoveNext());

        comparer.ThrowOnComparison(throwOnComparison);
        Assert.Throws<ComparerException>(operation);
        comparer.StopThrowing();

        Assert.Equal(count, heap.Count);
        Assert.Equal(items, heap.ToArray());
        Assert.True(enumerator.MoveNext());

        var drained = new List<int>();
        while (heap.TryPop(out var item))
            drained.Add(item);
        Assert.Equal(items.OrderBy(x => x), drained);
    }
}
