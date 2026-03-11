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
/// such as <see cref="RemoveOne(T)"/> and <see cref="IndexOf(T)"/> rely on
/// <see cref="EqualityComparer{T}.Default"/> to determine exact element matches.<br/>
/// This means that multiple distinct elements may compare equal for ordering
/// but still be treated as different values for removal or lookup.
/// </para>
///
/// <para>
/// The collection provides efficient range operations through
/// <see cref="LowerBound(T)"/>, <see cref="UpperBound(T)"/>, and
/// <see cref="Between(T, T)"/>.
/// </para>
/// </remarks>
[DebuggerDisplay("Count = {Count}")]
public class OrderedList<T> : IList<T>, IReadOnlyList<T>
{
    private readonly List<T> _list;
    private readonly IComparer<T> _comparer;

    public OrderedList(int capacity = 4, IComparer<T>? comparer = null)
    {
        _list = new List<T>(capacity);
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

    public int Count => _list.Count;

    public bool IsReadOnly => false;

    /// <summary>
    /// Gets the element at the specified index.
    /// </summary>
    /// <remarks>
    /// The list is always kept sorted; assigning a value is not supported.
    /// </remarks>
    public T this[int index]
    {
        get => _list[index];
        set => throw new NotSupportedException("Cannot set item in OrderedList.");
    }

    public void Add(T item)
    {
        // Fast path for appending to the end if the new item is greater than or equal to the last item.
        if (_list.Count == 0
            || _comparer.Compare(_list[^1], item) <= 0)
        {
            _list.Add(item);
            return;
        }

        var index = UpperBound(item);
        _list.Insert(index, item);
    }

    bool ICollection<T>.Remove(T item)
    {
        return RemoveOne(item);
    }

    public void RemoveAt(int index)
    {
        _list.RemoveAt(index);
    }

    public void Clear()
    {
        _list.Clear();
    }

    public bool Contains(T item)
    {
        return IndexOf(item) >= 0;
    }

    public int IndexOf(T item)
    {
        var start = LowerBound(item);
        var end = UpperBound(item);

        for (var i = start; i < end; i++)
        {
            // The comparer may only compare keys (for ordering), so we use EqualityComparer<T>.Default
            // to check actual equality when multiple items compare equal.
            if (EqualityComparer<T>.Default.Equals(_list[i], item))
                return i;
        }

        return -1;
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        _list.CopyTo(array, arrayIndex);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// This operation is not supported because the list must remain sorted.
    /// </summary>
    void IList<T>.Insert(int index, T item)
    {
        throw new NotSupportedException("Cannot insert at arbitrary position in OrderedList.");
    }

    /// <summary>
    /// Removes the first element that is equal to the specified item.
    /// </summary>
    /// <remarks>
    /// Equality is determined by <see cref="EqualityComparer{T}.Default"/>.
    /// </remarks>
    public bool RemoveOne(T item)
    {
        var index = IndexOf(item);

        if (index < 0)
            return false;

        _list.RemoveAt(index);
        return true;
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
        Check.NotNull(items);

        if (_list.Count == 0)
        {
            _list.AddRange(items);
            StableSort(_list, _comparer);
            return;
        }

        // Materialize and sort the new items.
        var temp = new List<T>(items);
        StableSort(temp, _comparer);

        // Fast path for appending to the end if the new items are all greater than or equal to the last item.
        if (_list.Count > 0 &&
            _comparer.Compare(_list[^1], temp[0]) <= 0)
        {
            _list.AddRange(temp);
            return;
        }

        // merge
        var merged = new List<T>(_list.Count + temp.Count);

        int i = 0, j = 0;

        while (i < _list.Count && j < temp.Count)
        {
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (_comparer.Compare(_list[i], temp[j]) <= 0)
                merged.Add(_list[i++]);
            else
                merged.Add(temp[j++]);
        }

        while (i < _list.Count)
            merged.Add(_list[i++]);

        while (j < temp.Count)
            merged.Add(temp[j++]);

        _list.Clear();
        _list.AddRange(merged);
    }

    /// <summary>
    /// Returns the index of the first element that is greater than the specified item.
    /// </summary>
    public int UpperBound(T item)
    {
        // Cannot rely on List<T>.BinarySearch because it may return any matching
        // index when duplicates exist. LowerBound must return the first element
        // >= item, so we perform the binary search manually.

        var low = 0;
        var high = _list.Count;

        while (low < high)
        {
            var mid = low + ((high - low) >> 1);

            if (_comparer.Compare(_list[mid], item) <= 0)
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
    public int LowerBound(T item)
    {
        // Cannot rely on List<T>.BinarySearch because it may return any matching
        // index when duplicates exist. LowerBound must return the first element
        // >= item, so we perform the binary search manually.

        var low = 0;
        var high = _list.Count;

        while (low < high)
        {
            var mid = low + ((high - low) >> 1);

            if (_comparer.Compare(_list[mid], item) < 0)
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
        var start = LowerBound(min);

        for (var i = start; i < _list.Count; i++)
        {
            if (_comparer.Compare(_list[i], max) > 0)
                yield break;

            yield return _list[i];
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
        var start = LowerBound(item);
        var end = UpperBound(item);

        var count = end - start;

        if (count > 0)
            _list.RemoveRange(start, count);

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
        return UpperBound(item) - LowerBound(item);
    }

    /// <summary>
    /// Returns all elements that compare equal to the specified item.
    /// </summary>
    public IEnumerable<T> EqualRange(T item)
    {
        var start = LowerBound(item);
        var end = UpperBound(item);

        for (var i = start; i < end; i++)
            yield return _list[i];
    }

    /// <summary>
    /// Removes all elements whose values fall within the specified range, inclusive.
    /// </summary>
    public int RemoveRange(T min, T max)
    {
        var start = LowerBound(min);
        var end = UpperBound(max);

        var count = end - start;

        if (count > 0)
            _list.RemoveRange(start, count);

        return count;
    }

    public int EnsureCapacity(int capacity)
    {
        Check.NotNegative(capacity);

        if (_list.Capacity < capacity)
        {
            _list.Capacity = capacity;
        }

        return _list.Capacity;
    }

    /// <summary>
    /// Performs a stable sort using the specified comparer.
    /// </summary>
    /// <remarks>
    /// <see cref="List{T}.Sort(IComparer{T})"/> is not guaranteed to be stable, so this method
    /// preserves the relative order of elements that compare equal.
    /// </remarks>
    private static void StableSort(List<T> list, IComparer<T> comparer)
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
}
