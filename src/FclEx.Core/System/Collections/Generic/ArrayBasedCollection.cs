namespace System.Collections.Generic;

public abstract class ArrayBasedCollection<TSelf, T> : IReadOnlyCollection<T> 
    where TSelf : ArrayBasedCollection<TSelf, T>
{
    protected const int DefaultCapacity = 4;

    protected T[] _items = [];
    protected int _count;
    protected int _version;

    // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
    public int Count => _count;

    /// <summary>
    /// Increase the capacity of this collection to at least the specified <paramref name="capacity"/>.
    /// </summary>
    /// <param name="capacity">The minimum capacity to ensure.</param>
    private int GetNewCapacity(int capacity)
    {
        Debug.Assert(_items.Length < capacity);

        var newCapacity = _items.Length == 0
            ? DefaultCapacity
            : 2 * _items.Length;

        // Allow the collection to grow to maximum possible capacity (~2G elements) before encountering overflow.
        // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
        if ((uint)newCapacity > Array.MaxLength)
            newCapacity = Array.MaxLength;

        // If the computed capacity is still less than specified, set to the original argument.
        // Capacities exceeding Array.MaxLength will be surfaced as OutOfMemoryException by Array.Resize.
        if (newCapacity < capacity)
            newCapacity = capacity;

        return newCapacity;
    }

    /// <summary>
    /// Increase the capacity of this collection to at least the specified <paramref name="capacity"/>.
    /// </summary>
    /// <param name="capacity">The minimum capacity to ensure.</param>
    internal void Grow(int capacity)
    {
        Capacity = GetNewCapacity(capacity);
    }

    // Gets and sets the capacity of this list.  The capacity is the size of
    // the internal array used to hold items.  When set, the internal
    // array of the collection is reallocated to the given capacity.
    //
    public int Capacity
    {
        get => _items.Length;
        set
        {
            Check.NotLessThan(value, _count);

            if (value == _items.Length)
                return;

            if (value > 0)
            {
                var newItems = new T[value];
                if (_count > 0)
                {
                    Array.Copy(_items, newItems, _count);
                }
                _items = newItems;
            }
            else
            {
                _items = [];
            }
        }
    }

    /// <summary>
    /// Ensures that the capacity of this collection is at least the specified <paramref name="capacity"/>.
    /// If the current capacity of the collection is less than specified <paramref name="capacity"/>,
    /// the capacity is increased to at least <paramref name="capacity"/>.
    /// </summary>
    /// <param name="capacity">The minimum capacity to ensure.</param>
    /// <returns>The new capacity of this collection.</returns>
    public int EnsureCapacity(int capacity)
    {
        Check.NotNegative(capacity);

        if (_items.Length < capacity)
        {
            Grow(capacity);
        }

        return _items.Length;
    }

    /// <summary>
    /// Sets the capacity of the collection to match its current size, minimizing memory overhead
    /// when no additional elements are expected to be added.
    /// To completely clear the collection and release all referenced memory, call:
    /// <code>
    /// this.Clear();
    /// this.TrimExcess();
    /// </code>
    /// </summary>
    public void TrimExcess()
    {
        var threshold = (int)(((double)_items.Length) * 0.9);
        if (_count < threshold)
        {
            Capacity = _count;
        }
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        // Delegate rest of error checking to Array.Copy.
        Array.Copy(_items, 0, array, arrayIndex, _count);
    }

    public void Clear()
    {
        if (RuntimeHelpersEx.IsReferenceOrContainsReferences<T>())
            Array.Clear(_items, 0, _count);

        _count = 0;
        ++_version;
    }

    public Enumerator GetEnumerator() => new((TSelf)this);
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public struct Enumerator : IEnumerator<T>
    {
        private readonly TSelf _self;
        private readonly int _version;
        private int _index = -1;
        private T? _current;

        internal Enumerator(TSelf self)
        {
            _self = self;
            _version = self._version;
        }

        public readonly T Current => _current!;
        readonly object IEnumerator.Current => Current!;

        public bool MoveNext()
        {
            Check.VersionEqual(_self._version, _version);
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (++_index >= _self._count)
            {
                _current = default;
                return false;
            }

            _current = _self._items[_index];
            return true;
        }

        public void Reset()
        {
            Check.VersionEqual(_self._version, _version);
            _index = -1;
            _current = default;
        }

        public readonly void Dispose() { }
    }
}
