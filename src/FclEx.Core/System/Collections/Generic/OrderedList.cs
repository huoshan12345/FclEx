namespace System.Collections.Generic;

/// <summary>
/// Represents a list that maintains its elements in sorted order.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="OrderedList{T}"/> keeps elements sorted according to the specified
/// <see cref="IComparer{T}"/>. Elements may appear multiple times if they compare
/// equal according to the comparer.
/// </para>
///
/// <para>
/// Insertions preserve stability: when multiple elements compare equal,
/// newly added elements are placed after existing ones with the same sort key,
/// preserving their relative insertion order.
/// </para>
///
/// <para>
/// Ordering is determined by the comparer, while equality-based operations
/// such as <see cref="Remove(T)"/> and <see cref="IndexOf(T)"/> rely on
/// <see cref="EqualityComparer{T}.Default"/> to determine exact element matches.<br/>
/// This means that multiple distinct elements may compare equal for ordering
/// but still be treated as different values for removal or lookup.
/// </para>
///
/// <para>
/// The collection provides efficient range operations through
/// <see cref="LowerBound(T, int?, int?)"/>, <see cref="UpperBound(T, int?, int?)"/>, and
/// <see cref="Between(T, T)"/>.
/// </para>
/// </remarks>
[DebuggerDisplay("Count = {Count}")]
public class OrderedList<T> : IList<T>, IReadOnlyList<T>
{
    private readonly List<T> _items;
    private readonly IComparer<T> _comparer;
    private int _version;

    public OrderedList(int capacity = 4, IComparer<T>? comparer = null)
    {
        _items = new List<T>(capacity);
        _comparer = comparer ?? Comparer<T>.Default;
    }

    public OrderedList(IComparer<T> comparer) : this(4, comparer)
    {
    }

    public OrderedList(IEnumerable<T> items, IComparer<T>? comparer = null) : this(4, comparer)
    {
        _comparer = comparer ?? Comparer<T>.Default;
        AddRange(items);
    }

    public int Count => _items.Count;

    public bool IsReadOnly => false;

    /// <summary>
    /// Gets the element at the specified index.
    /// </summary>
    /// <remarks>
    /// The list is always kept sorted; assigning a value is not supported.
    /// </remarks>
    public T this[int index]
    {
        get => _items[index];
        set => throw new NotSupportedException("Cannot set item in OrderedList.");
    }

    public void Add(T item)
    {
        // Fast path for appending to the end if the new item is greater than or equal to the last item.
        if (_items.Count == 0
            || _comparer.Compare(_items[^1], item) <= 0)
        {
            _items.Add(item);
        }
        else
        {
            var index = UpperBound(item);
            _items.Insert(index, item);
        }

        ++_version;
    }

    public bool Remove(T item)
    {
        var index = IndexOf(item);
        if (index < 0)
            return false;

        _items.RemoveAt(index);

        ++_version;

        return true;
    }

    public void RemoveAt(int index)
    {
        _items.RemoveAt(index);
        ++_version;
    }

    public void Clear()
    {
        _items.Clear();
        ++_version;
    }

    public bool Contains(T item)
    {
        return IndexOf(item) >= 0;
    }

    private (int Lower, bool Equal) FindLowerBound(T item, int? lower = null, int? upper = null)
    {
        var index = LowerBound(item, lower, upper);
        var equal = index < _items.Count
                    && _comparer.Compare(_items[index], item) == 0;

        return (index, equal);
    }

    private (int Upper, bool EqualToPrev) FindUpperBound(T item, int? lower = null, int? upper = null)
    {
        var index = UpperBound(item, lower, upper);
        var equal = index > 0
                    && _comparer.Compare(_items[index - 1], item) == 0;

        return (index, equal);
    }

    public int IndexOf(T item)
    {
        return IndexOf(item, 0, _items.Count);
    }

    public int IndexOf(T item, int index)
    {
        return IndexOf(item, index, _items.Count - index);
    }

    public int IndexOf(T item, int index, int count)
    {
        // allow index = _item.Count and count = 0
        Check.Between(index, 0, _items.Count);
        Check.Between(count, 0, _items.Count - index);

        if (_items.Count == 0 || count == 0)
            return -1;

        var upper = index + count;
        var (lower, equal) = FindLowerBound(item, index, upper);

        return equal
            ? lower
            : -1;
    }

    public int LastIndexOf(T item)
    {
        if (_items.Count == 0)
            return -1;

        return LastIndexOf(item, _items.Count - 1, _items.Count);
    }

    public int LastIndexOf(T item, int index)
    {
        Check.LessThan(index, _items.Count);
        return LastIndexOf(item, index, index + 1);
    }

    public int LastIndexOf(T item, int index, int count)
    {
        if (_items.Count != 0)
        {
            Check.NotNegative(index);
            Check.NotNegative(count);
        }

        if (_items.Count == 0)
            return -1;

        Check.LessThan(index, _items.Count);
        Check.NotGreaterThan(count, index + 1);

        if (count == 0)
            return -1;

        var lower = index - count + 1;
        var (upper, equalToPrev) = FindUpperBound(item, lower, index + 1);

        return equalToPrev
            ? upper - 1
            : -1;
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        _items.CopyTo(array, arrayIndex);
    }

    public Enumerator GetEnumerator() => new(this);
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// This operation is not supported because the list must remain sorted.
    /// </summary>
    void IList<T>.Insert(int index, T item)
    {
        throw new NotSupportedException("Cannot insert at arbitrary position in OrderedList.");
    }

    /// <summary>
    /// Adds a sequence of elements to the list while preserving the sorted order.
    /// </summary>
    public void AddRange(IEnumerable<T> items)
    {
        /*
            Adds a range of items while preserving the sorted invariant of the list.

            Naively inserting each element via Add() would perform a binary search
            followed by an insertion for every item, resulting in O(m * n) behavior
            due to repeated shifting of elements in the underlying array.

            Instead, this method:

            1. Materializes the incoming sequence into a temporary list.
            2. Sorts the new items using the same comparer.
            3. Merges the existing sorted list and the newly sorted items
               using a linear merge step similar to the merge phase of merge sort.

            This approach reduces the complexity to:

                O(m log m + n)

            where:
                n = current element count
                m = number of items being added

            The merge step preserves ordering and avoids repeated internal shifts
            that would otherwise occur with repeated Insert operations.
        */

        Check.NotNull(items);

        var temp = new List<T>(items);
        if (temp.Count == 0)
            return;

        if (_items.Count == 0)
        {
            _items.AddRange(temp);
            StableSort(_items, _comparer);
            return;
        }

        // Materialize and sort the new items.

        StableSort(temp, _comparer);

        // Fast path for appending to the end if the new items are all greater than or equal to the last item.
        if (_items.Count > 0 &&
            _comparer.Compare(_items[^1], temp[0]) <= 0)
        {
            _items.AddRange(temp);
            ++_version;

            return;
        }

        // merge
        var merged = new List<T>(_items.Count + temp.Count);

        int i = 0, j = 0;

        while (i < _items.Count && j < temp.Count)
        {
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (_comparer.Compare(_items[i], temp[j]) <= 0)
                merged.Add(_items[i++]);
            else
                merged.Add(temp[j++]);
        }

        while (i < _items.Count)
            merged.Add(_items[i++]);

        while (j < temp.Count)
            merged.Add(temp[j++]);

        _items.Clear();
        _items.AddRange(merged);

        ++_version;
    }

    /// <summary>
    /// Returns the index of the first element that is greater than the specified item.
    /// </summary>
    public int UpperBound(T item, int? lower = null, int? upper = null)
    {
        // Cannot rely on List<T>.BinarySearch because it may return any matching
        // index when duplicates exist. LowerBound must return the first element
        // >= item, so we perform the binary search manually.

        var low = lower ?? 0;
        var high = upper ?? _items.Count;

        while (low < high)
        {
            var mid = low + ((high - low) >> 1);

            if (_comparer.Compare(_items[mid], item) <= 0)
            {
                low = mid + 1;
            }
            else
            {
                high = mid;
            }
        }

        return low;
    }

    /// <summary>
    /// Returns the index of the first element that is greater than or equal to the specified item.
    /// </summary>
    public int LowerBound(T item, int? lower = null, int? upper = null)
    {
        // Cannot rely on List<T>.BinarySearch because it may return any matching
        // index when duplicates exist. LowerBound must return the first element
        // >= item, so we perform the binary search manually.

        var low = lower ?? 0;
        var high = upper ?? _items.Count;

        while (low < high)
        {
            var mid = low + ((high - low) >> 1);

            if (_comparer.Compare(_items[mid], item) < 0)
            {
                low = mid + 1;
            }
            else
            {
                high = mid;
            }
        }

        return low;
    }

    /// <summary>
    /// Returns all elements whose values are between the specified minimum and maximum, inclusive.
    /// </summary>
    public IEnumerable<T> Between(T min, T max)
    {
        var start = IndexOf(min);
        var end = LastIndexOf(max);

        for (var i = start; i < end; i++)
        {
            yield return _items[i];
        }
    }

    /// <summary>
    /// Removes all elements that are equal to the specified item.
    /// </summary>
    /// <remarks>
    /// Equality is determined by <see cref="EqualityComparer{T}.Default"/>.
    /// </remarks>
    public int RemoveAll(T item)
    {
        var start = IndexOf(item);
        var end = LastIndexOf(item);

        var count = end - start;

        // ReSharper disable once InvertIf
        if (count > 0)
        {
            _items.RemoveRange(start, count);
            ++_version;
        }

        return count;
    }

    /// <summary>
    /// Returns the number of elements that are equal to the specified item.
    /// </summary>
    /// <remarks>
    /// Equality is determined by <see cref="EqualityComparer{T}.Default"/>.
    /// </remarks>
    public int CountOf(T item)
    {
        return LastIndexOf(item) - IndexOf(item);
    }

    /// <summary>
    /// Returns all elements that compare equal to the specified item.
    /// </summary>
    public IEnumerable<T> EqualRange(T item)
    {
        var start = IndexOf(item);
        var end = LastIndexOf(item);

        for (var i = start; i < end; i++)
            yield return _items[i];
    }

    /// <summary>
    /// Removes all elements whose values fall within the specified range, inclusive.
    /// </summary>
    public int RemoveRange(T min, T max)
    {
        var start = IndexOf(min);
        var end = LastIndexOf(max);

        var count = end - start;

        // ReSharper disable once InvertIf
        if (count > 0)
        {
            _items.RemoveRange(start, count);
            ++_version;
        }

        return count;
    }

    public int EnsureCapacity(int capacity)
    {
        Check.NotNegative(capacity);

        if (_items.Capacity < capacity)
        {
            _items.Capacity = capacity;
        }

        return _items.Capacity;
    }

    /// <summary>
    /// Performs a stable sort using the specified comparer.
    /// </summary>
    /// <remarks>
    /// <see cref="List{T}.Sort(IComparer{T})"/> is not guaranteed to be stable, so this method
    /// preserves the relative order of elements that compare equal.
    /// </remarks>
    internal static void StableSort(List<T> list, IComparer<T> comparer)
    {
        var n = list.Count;

        var index = new int[n];

        for (var i = 0; i < n; i++)
            index[i] = i;

        Array.Sort(index, (a, b) =>
        {
            var c = comparer.Compare(list[a], list[b]);
            return c != 0 ? c : a.CompareTo(b);
        });

        var temp = list.ToArray();

        for (var i = 0; i < n; i++)
            list[i] = temp[index[i]];
    }

    public struct Enumerator : IEnumerator<T>
    {
        private readonly OrderedList<T> _list;
        private readonly int _version;
        private int _index = -1;
        private T? _current;

        internal Enumerator(OrderedList<T> list)
        {
            _list = list;
            _version = list._version;
        }

        public readonly T Current => _current!;
        readonly object IEnumerator.Current => Current!;

        public bool MoveNext()
        {
            Check.VersionEqual(_list._version, _version);
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (++_index >= _list._items.Count)
            {
                _current = default;
                return false;
            }

            _current = _list._items[_index];
            return true;
        }

        public void Reset()
        {
            Check.VersionEqual(_list._version, _version);
            _index = -1;
            _current = default;
        }

        public readonly void Dispose() { }
    }
}
