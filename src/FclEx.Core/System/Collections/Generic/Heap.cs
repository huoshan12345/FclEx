// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable ConvertToAutoPropertyWhenPossible
namespace System.Collections.Generic;

/// <summary>
/// Represents a min-heap based priority queue implemented as a 4-ary heap.
/// </summary>
/// <typeparam name="T">The type of elements stored in the heap.</typeparam>
/// <remarks>
/// Elements are ordered according to the specified <see cref="IComparer{T}"/>.<br/>
/// The smallest element can be inspected with <see cref="Peek"/> or removed with <see cref="Pop"/>.<br/>
/// <br/>
/// This implementation uses a 4-ary heap to reduce heap height and improve cache locality.<br/>
/// <br/>
/// Enumeration iterates over the internal storage and does not return elements
/// in sorted or priority order.<br/>
/// <br/>
/// Typical time complexities:
/// <list type="bullet">
/// <item><description><see cref="Push"/>: O(log n)</description></item>
/// <item><description><see cref="Pop"/>: O(log n)</description></item>
/// <item><description><see cref="Peek"/>: O(1)</description></item>
/// <item><description>Heap construction from a collection: O(n)</description></item>
/// </list>
/// </remarks>
[DebuggerDisplay("Count = {Count}")]
public class Heap<T> : ArrayBasedCollection<Heap<T>, T>, ICollection<T>
{
    private const int Arity = 4;
    private const int MaxHeapHeight = 32;

    private readonly IComparer<T> _comparer;

    public Heap(int capacity, IComparer<T>? comparer = null)
    {
        Check.NotNegative(capacity);

        _items = capacity == 0
            ? []
            : new T[capacity];

        _comparer = comparer ?? Comparer<T>.Default;
    }

    public Heap(IComparer<T>? comparer = null) : this(0, comparer)
    {
    }

    public Heap(IEnumerable<T> items, IComparer<T>? comparer = null)
    {
        Check.NotNull(items);

        _comparer = comparer ?? Comparer<T>.Default;
        _items = items.ToArray();
        _count = _items.Length;

        if (_count > 1)
        {
            Heapify();
        }
    }

    public IComparer<T> Comparer => _comparer;
    public bool IsReadOnly => false;

    void ICollection<T>.Add(T item) => Push(item);

    bool ICollection<T>.Contains(T item)
    {
        return Array.IndexOf(_items, item, 0, _count) >= 0;
    }

    bool ICollection<T>.Remove(T item)
    {
        throw new NotSupportedException("Removing specific items is not supported by Heap<T>.");
    }

    /// <summary>
    /// Inserts an element into the heap.
    /// </summary>
    public void Push(T item)
    {
        var index = _count;
        var destination = FindSiftUpDestination(index, item);

        if (_count == _items.Length)
            Grow(_count + 1);

        CommitSiftUp(index, destination, item);
        _count++;
        _version++;
    }

    public void PushRange(IEnumerable<T> items)
    {
        Check.NotNull(items);

        if (items.TryGetNonEnumeratedCount(out var count))
        {
            if (count == 0)
                return;

            if (count > Capacity - _count)
            {
                Grow(checked(_count + count));
            }
        }

        foreach (var item in items)
        {
            Push(item);
        }
    }

    [MethodImpl(AggressiveInlining)]
    private void EnsureNotEmpty()
    {
        if (_count == 0)
            throw new InvalidOperationException("Heap empty.");
    }

    /// <summary>
    /// Removes and returns the smallest element in the heap.
    /// </summary>
    /// <exception cref="InvalidOperationException">The heap is empty.</exception>
    public T Pop()
    {
        EnsureNotEmpty();

        var last = _count - 1;
        var root = _items[0];

        if (last > 0)
        {
            var item = _items[last];
            Span<int> path = stackalloc int[MaxHeapHeight];
            var pathLength = FindSiftDownPath(0, item, last, path);
            CommitSiftDown(0, item, path[..pathLength]);
        }

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            _items[last] = default!;

        _count = last;
        _version++;
        return root;
    }

    /// <summary>
    /// Attempts to remove and return the smallest element in the heap.
    /// </summary>
    /// <returns><see langword="true"/> if an element was removed; otherwise <see langword="false"/>.</returns>
    public bool TryPop(out T value)
    {
        if (_count == 0)
        {
            value = default!;
            return false;
        }

        value = Pop();
        return true;
    }

    /// <summary>
    /// Returns the smallest element in the heap without removing it.
    /// </summary>
    /// <exception cref="InvalidOperationException">The heap is empty.</exception>
    public T Peek()
    {
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (_count == 0)
            throw new InvalidOperationException();

        return _items[0];
    }

    /// <summary>
    /// Attempts to return the smallest element in the heap without removing it.
    /// </summary>
    /// <returns><see langword="true"/> if the heap is not empty; otherwise <see langword="false"/>.</returns>
    public bool TryPeek(out T value)
    {
        if (_count == 0)
        {
            value = default!;
            return false;
        }

        value = _items[0];
        return true;
    }

    /// <summary>
    /// Replaces the smallest element with the specified item and returns the previous smallest element.
    /// </summary>
    /// <exception cref="InvalidOperationException">The heap is empty.</exception>
    public T PopPush(T item)
    {
        EnsureNotEmpty();

        var root = _items[0];
        SiftDown(0, item);
        return root;
    }

    /// <summary>
    /// Inserts an element and removes the smallest element in a single operation.
    /// </summary>
    /// <returns>The element that was removed.</returns>
    public T PushPop(T item)
    {
        if (_count == 0 || _comparer.Compare(item, _items[0]) <= 0)
            return item;

        var root = _items[0];
        SiftDown(0, item);
        return root;
    }

    private void Heapify()
    {
        var start = Parent(_count - 1);

        for (var i = start; i >= 0; i--)
        {
            SiftDown(i, _items[i]);
        }
    }

    private static int Parent(int i) => (i - 1) / Arity;

    private int FindSiftUpDestination(int index, T item)
    {
        var destination = index;
        while (destination > 0)
        {
            var parent = Parent(destination);

            if (_comparer.Compare(item, _items[parent]) >= 0)
                break;

            destination = parent;
        }

        return destination;
    }

    private void CommitSiftUp(int index, int destination, T item)
    {
        while (index > destination)
        {
            var parent = Parent(index);
            _items[index] = _items[parent];
            index = parent;
        }

        _items[destination] = item;
    }

    private void SiftDown(int i, T item)
    {
        Span<int> path = stackalloc int[MaxHeapHeight];
        var pathLength = FindSiftDownPath(i, item, _count, path);
        CommitSiftDown(i, item, path[..pathLength]);
        _version++;
    }

    private int FindSiftDownPath(int index, T item, int count, Span<int> path)
    {
        var pathLength = 0;
        while (true)
        {
            var firstCandidate = (long)index * Arity + 1;
            if (firstCandidate >= count)
                break;

            var first = (int)firstCandidate;
            var best = first;
            var bestValue = _items[first];
            var last = first + Arity;

            if (last > count)
                last = count;

            for (var j = first + 1; j < last; j++)
            {
                var v = _items[j];
                if (_comparer.Compare(v, bestValue) < 0)
                {
                    best = j;
                    bestValue = v;
                }
            }

            if (_comparer.Compare(bestValue, item) >= 0)
                break;

            Debug.Assert(pathLength < path.Length);
            path[pathLength++] = best;
            index = best;
        }

        return pathLength;
    }

    private void CommitSiftDown(int index, T item, ReadOnlySpan<int> path)
    {
        foreach (var child in path)
        {
            _items[index] = _items[child];
            index = child;
        }

        _items[index] = item;
    }
}
