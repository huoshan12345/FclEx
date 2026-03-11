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
public class Heap<T> : IReadOnlyCollection<T>
{
    private const int Arity = 4;

    private T[] _data;
    private int _count;
    private readonly IComparer<T> _comparer;

    public Heap(int capacity = 4, IComparer<T>? comparer = null)
    {
        if (capacity < 4) 
            capacity = 4;

        _data = new T[capacity];
        _comparer = comparer ?? Comparer<T>.Default;
    }

    public Heap(IComparer<T> comparer) : this(4, comparer)
    {
    }

    public Heap(IEnumerable<T> items, IComparer<T>? comparer = null)
    {
        _comparer = comparer ?? Comparer<T>.Default;
        _data = items.ToArray();
        _count = _data.Length;

        if (_data.Length < 4)
            Array.Resize(ref _data, 4);

        Heapify();
    }

    // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
    public int Count => _count;

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <remarks>
    /// Enumeration does not return elements in sorted order.
    /// </remarks>
    public ArrayEnumerator<T> GetEnumerator()
    {
        return new ArrayEnumerator<T>(_data, 0, _count);
    }

    /// <summary>
    /// Gets the total number of elements the internal storage can hold without resizing.
    /// </summary>
    public int Capacity => _data.Length;

    /// <summary>
    /// Removes all elements from the heap.
    /// </summary>
    public void Clear()
    {
        if (RuntimeHelpersEx.IsReferenceOrContainsReferences<T>())
            Array.Clear(_data, 0, _count);

        _count = 0;
    }

    /// <summary>
    /// Ensures the heap can hold at least the specified number of elements without resizing.
    /// </summary>
    public void EnsureCapacity(int capacity)
    {
        Check.NotNegative(capacity);

        if (_data.Length < capacity)
            Array.Resize(ref _data, capacity);
    }

    /// <summary>
    /// Inserts an element into the heap.
    /// </summary>
    public void Push(T item)
    {
        var count = _count;

        if (count == _data.Length)
        {
            Grow();
        }

        _count = count + 1;
        SiftUp(count, item);
    }

    /// <summary>
    /// Removes and returns the smallest element in the heap.
    /// </summary>
    /// <exception cref="InvalidOperationException">The heap is empty.</exception>
    public T Pop()
    {
        if (_count == 0)
            throw new InvalidOperationException();

        var last = --_count;

        var root = _data[0];
        var x = _data[last];

        if (RuntimeHelpersEx.IsReferenceOrContainsReferences<T>())
            _data[last] = default!;

        if (last > 0)
            SiftDown(0, x);

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
        if (_count == 0)
            throw new InvalidOperationException();

        return _data[0];
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

        value = _data[0];
        return true;
    }

    /// <summary>
    /// Replaces the smallest element with the specified item and returns the previous smallest element.
    /// </summary>
    /// <remarks>
    /// If the heap is empty, the item is inserted and returned.
    /// </remarks>
    public T ReplaceTop(T item)
    {
        if (_count == 0)
        {
            Push(item);
            return item;
        }

        var data = _data;
        var root = data[0];

        SiftDown(0, item);

        return root;
    }

    /// <summary>
    /// Attempts to replace the smallest element with the specified item.
    /// </summary>
    /// <returns><see langword="true"/> if the heap was not empty; otherwise <see langword="false"/>.</returns>
    public bool TryReplaceTop(T item, out T? old)
    {
        if (_count == 0)
        {
            old = default;
            Push(item);
            return false;
        }

        old = _data[0];
        SiftDown(0, item);
        return true;
    }

    /// <summary>
    /// Inserts an element and removes the smallest element in a single operation.
    /// </summary>
    /// <returns>The element that was removed.</returns>
    public T PushPop(T item)
    {
        if (_count == 0 || _comparer.Compare(item, _data[0]) <= 0)
            return item;

        var root = _data[0];
        SiftDown(0, item);
        return root;
    }

    /// <summary>
    /// Sets the capacity to the actual number of elements, or to the specified capacity if provided.
    /// </summary>
    public void TrimExcess(int? capacity = null)
    {
        var count = _count;

        var newCapacity = capacity ?? count;

        if (newCapacity < count)
            newCapacity = count;

        if (newCapacity < 4)
            newCapacity = 4;

        if (newCapacity >= _data.Length)
            return;

        Array.Resize(ref _data, newCapacity);
    }

    private void Heapify()
    {
        var start = Parent(_count - 1);

        for (var i = start; i >= 0; i--)
        {
            SiftDown(i, _data[i]);
        }
    }

    private void Grow()
    {
        var newSize = _data.Length * 2;
        if (newSize < 4)
            newSize = 4;

        if ((uint)newSize > Array.MaxLength)
            newSize = Array.MaxLength;

        Array.Resize(ref _data, newSize);
    }

    private static int Parent(int i) => (i - 1) / Arity;

    private static int FirstChild(int i) => i * Arity + 1;

    private void SiftUp(int i, T item)
    {
        while (i > 0)
        {
            var parent = Parent(i);
            var p = _data[parent];

            if (_comparer.Compare(item, p) >= 0)
                break;

            _data[i] = p;
            i = parent;
        }

        _data[i] = item;
    }

    private void SiftDown(int i, T item)
    {
        while (true)
        {
            var first = FirstChild(i);
            if (first >= _count)
                break;

            var best = first;
            var bestValue = _data[first];
            var last = first + Arity;

            if (last > _count)
                last = _count;

            for (var j = first + 1; j < last; j++)
            {
                var v = _data[j];
                if (_comparer.Compare(_data[j], bestValue) < 0)
                {
                    best = j;
                    bestValue = v;
                }
            }

            var child = _data[best];

            if (_comparer.Compare(child, item) >= 0)
                break;

            _data[i] = child;
            i = best;
        }

        _data[i] = item;
    }
}
