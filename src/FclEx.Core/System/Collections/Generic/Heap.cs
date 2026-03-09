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
public class Heap<T> : ICollection<T>
{
    private const int Arity = 4;

    private T[] _items;
    private int _count;
    private readonly IComparer<T> _comparer;
    private int _version;

    public Heap(int capacity = 4, IComparer<T>? comparer = null)
    {
        if (capacity < 4)
            capacity = 4;

        _items = new T[capacity];
        _comparer = comparer ?? Comparer<T>.Default;
    }

    public Heap(IComparer<T> comparer) : this(4, comparer)
    {
    }

    public Heap(IEnumerable<T> items, IComparer<T>? comparer = null)
    {
        _comparer = comparer ?? Comparer<T>.Default;
        _items = items.ToArray();
        _count = _items.Length;

        if (_items.Length < 4)
            Array.Resize(ref _items, 4);

        Heapify();
    }

    // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
    public int Count => _count;

    /// <remarks>
    /// Enumeration does not return elements in sorted order.
    /// </remarks>
    public Enumerator GetEnumerator() => new(this);
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool IsReadOnly => false;

    /// <summary>
    /// Removes all elements from the heap.
    /// </summary>
    public void Clear()
    {
        if (RuntimeHelpersEx.IsReferenceOrContainsReferences<T>())
            Array.Clear(_items, 0, _count);

        _count = 0;
        ++_version;
    }

    void ICollection<T>.Add(T item) => Push(item);

    bool ICollection<T>.Contains(T item)
    {
        return Array.IndexOf(_items, item, 0, _count) >= 0;
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        // Delegate rest of error checking to Array.Copy.
        Array.Copy(_items, 0, array, arrayIndex, _count);
    }

    bool ICollection<T>.Remove(T item)
    {
        throw new NotSupportedException("Removing specific items is not supported by Heap<T>.");
    }

    /// <summary>
    /// Gets the total number of elements the internal storage can hold without resizing.
    /// </summary>
    public int Capacity => _items.Length;

    /// <summary>
    /// Ensures the heap can hold at least the specified number of elements without resizing.
    /// </summary>
    public void EnsureCapacity(int capacity)
    {
        Check.NotNegative(capacity);

        if (_items.Length < capacity)
            Array.Resize(ref _items, capacity);
    }

    /// <summary>
    /// Inserts an element into the heap.
    /// </summary>
    public void Push(T item)
    {
        if (_count == _items.Length)
            Grow();

        var index = _count++;
        SiftUp(index, item);
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

        var root = _items[0];
        var x = _items[last];

        if (RuntimeHelpersEx.IsReferenceOrContainsReferences<T>())
            _items[last] = default!;

        if (last > 0)
        {
            SiftDown(0, x); // ShiftDown also increments version
        }
        else
        {
            ++_version;
        }

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

        var data = _items;
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

        old = _items[0];
        SiftDown(0, item);

        return true;
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

        if (newCapacity >= _items.Length)
            return;

        Array.Resize(ref _items, newCapacity);
    }

    private void Heapify()
    {
        var start = Parent(_count - 1);

        for (var i = start; i >= 0; i--)
        {
            SiftDown(i, _items[i]);
        }
    }

    private void Grow()
    {
        var newSize = _items.Length * 2;
        if (newSize < 4)
            newSize = 4;

        if ((uint)newSize > Array.MaxLength)
            newSize = Array.MaxLength;

        Array.Resize(ref _items, newSize);
    }

    private static int Parent(int i) => (i - 1) / Arity;

    private static int FirstChild(int i) => i * Arity + 1;

    private void SiftUp(int i, T item)
    {
        while (i > 0)
        {
            var parent = Parent(i);
            var p = _items[parent];

            if (_comparer.Compare(item, p) >= 0)
                break;

            _items[i] = p;
            i = parent;
        }

        _items[i] = item;

        ++_version;
    }

    private void SiftDown(int i, T item)
    {
        while (true)
        {
            var first = FirstChild(i);
            if (first >= _count)
                break;

            var best = first;
            var bestValue = _items[first];
            var last = first + Arity;

            if (last > _count)
                last = _count;

            for (var j = first + 1; j < last; j++)
            {
                var v = _items[j];
                if (_comparer.Compare(_items[j], bestValue) < 0)
                {
                    best = j;
                    bestValue = v;
                }
            }

            var child = _items[best];

            if (_comparer.Compare(child, item) >= 0)
                break;

            _items[i] = child;
            i = best;
        }

        _items[i] = item;

        ++_version;
    }

    public struct Enumerator : IEnumerator<T>
    {
        private readonly Heap<T> _heap;
        private readonly int _version;
        private int _index = -1;
        private T? _current;

        internal Enumerator(Heap<T> heap)
        {
            _heap = heap;
            _version = heap._version;
        }

        public readonly T Current => _current!;
        readonly object IEnumerator.Current => Current!;

        public bool MoveNext()
        {
            Check.VersionEqual(_heap._version, _version);
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (++_index >= _heap._count)
            {
                _current = default;
                return false;
            }

            _current = _heap._items[_index];
            return true;
        }

        public void Reset()
        {
            Check.VersionEqual(_heap._version, _version);
            _index = -1;
            _current = default;
        }

        public readonly void Dispose() { }
    }
}
