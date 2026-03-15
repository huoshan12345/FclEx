// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable ConvertToAutoPropertyWhenPossible
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
/// <see cref="IComparer{T}"/> used by the collection to determine exact element matches.<br/>
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
public class OrderedList<T> : ArrayBasedCollection<OrderedList<T>, T>, IList<T>, IReadOnlyList<T>
{
    private readonly IComparer<T> _comparer;

    public OrderedList(int capacity = 0, IComparer<T>? comparer = null)
    {
        Check.NotNegative(capacity);

        // _items is initialized to an empty array in the base class, so only allocate a new array if capacity > 0
        if (capacity != 0)
            _items = new T[capacity];

        _comparer = comparer ?? Comparer<T>.Default;
    }

    public OrderedList(IComparer<T> comparer) : this(0, comparer)
    {
    }

    public OrderedList(IEnumerable<T> items, IComparer<T>? comparer = null) : this(4, comparer)
    {
        _comparer = comparer ?? Comparer<T>.Default;
        AddRange(items);
    }

    public IComparer<T> Comparer => _comparer;
    public bool IsReadOnly => false;

    /// <summary>
    /// Gets the element at the specified index.
    /// </summary>
    /// <remarks>
    /// The list is always kept sorted; assigning a value is not supported.
    /// </remarks>
    public T this[int index]
    {
        get
        {
            Check.Between(index, 0, _count - 1);
            return _items[index];
        }
        set => throw new NotSupportedException("Cannot set item in OrderedList.");
    }

    public void Add(T item)
    {
        if (_count == _items.Length)
            Grow(_count + 1);

        int index;

        if (_count == 0
            || _comparer.Compare(_items[_count - 1], item) <= 0)
        {
            index = _count;
        }
        else
        {
            index = UpperBound(item);
            Array.Copy(_items, index, _items, index + 1, _count - index);
        }

        _items[index] = item;
        ++_count;
        ++_version;
    }

    bool ICollection<T>.Remove(T item)
    {
        var index = IndexOf(item);
        if (index < 0)
            return false;

        RemoveAt(index);

        return true;
    }

    public void RemoveAt(int index)
    {
        Check.Between(index, 0, _count - 1);

        --_count;

        if (index < _count)
        {
            Array.Copy(_items, index + 1, _items, index, _count - index);
        }
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            _items[_count] = default!;
        }

        ++_version;
    }

    public bool Contains(T item)
    {
        return IndexOf(item) >= 0;
    }

    private (int Lower, bool Equal) FindLowerBound(T item, int? lower = null, int? upper = null)
    {
        var index = LowerBound(item, lower, upper);
        var equal = index < _count
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
        return IndexOf(item, 0, _count);
    }

    public int IndexOf(T item, int index)
    {
        return IndexOf(item, index, _count - index);
    }

    public int IndexOf(T item, int index, int count)
    {
        // allow index = _item.Count and count = 0
        Check.Between(index, 0, _count);
        Check.Between(count, 0, _count - index);

        if (_count == 0 || count == 0)
            return -1;

        var upper = index + count;
        var (lower, equal) = FindLowerBound(item, index, upper);

        return equal
            ? lower
            : -1;
    }

    public int LastIndexOf(T item)
    {
        if (_count == 0)
            return -1;

        return LastIndexOf(item, _count - 1, _count);
    }

    public int LastIndexOf(T item, int index)
    {
        Check.LessThan(index, _count);
        return LastIndexOf(item, index, index + 1);
    }

    public int LastIndexOf(T item, int index, int count)
    {
        if (_count != 0)
        {
            Check.NotNegative(index);
            Check.NotNegative(count);
        }

        if (_count == 0)
            return -1;

        Check.LessThan(index, _count);
        Check.NotGreaterThan(count, index + 1);

        if (count == 0)
            return -1;

        var lower = index - count + 1;
        var (upper, equalToPrev) = FindUpperBound(item, lower, index + 1);

        return equalToPrev
            ? upper - 1
            : -1;
    }

    /// <summary>
    /// This operation is not supported because the list must remain sorted.
    /// </summary>
    void IList<T>.Insert(int index, T item)
    {
        throw new NotSupportedException("Cannot insert at arbitrary position in OrderedList.");
    }

    private void AppendSorted(T[] items)
    {
        var count = items.Length;
        var requiredCapacity = _count + count;
        if (_items.Length < requiredCapacity)
        {
            Grow(checked(_count + count));
        }

        items.CopyTo(_items, _count);
        _count += count;
        _version++;
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

        if (items.TryGetNonEnumeratedCount(out var c) && c == 0)
            return;

        // always create a new array to avoid mutating the input collection if it's already an array or list
        var temp = items.ToArray();

        // Materialize and sort the new items.
        temp.StableSort(_comparer);

        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (_count == 0)
        {
            // If the list is currently empty, we can skip the merge step and just take the new items as-is.
            _items = temp;
            _count = temp.Length;
            ++_version;
            return;
        }

        // Fast path for appending to the end if the new items are all greater than or equal to the last item.
        if (_count > 0 &&
            _comparer.Compare(_items[_count - 1], temp[0]) <= 0)
        {
            AppendSorted(temp);
            return;
        }

        // merge
        var merged = new T[_count + temp.Length];

        int i = 0, j = 0, count = 0;

        while (i < _count && j < temp.Length)
        {
            merged[count++] = _comparer.Compare(_items[i], temp[j]) <= 0
                ? _items[i++]
                : temp[j++];
        }

        while (i < _count)
        {
            merged[count++] = _items[i++];
        }

        while (j < temp.Length)
        {
            merged[count++] = temp[j++];
        }

        _items = merged;
        _count = count;
        ++_version;
    }

    /// <summary>
    /// Returns the index of the first element that is greater than the specified item.
    /// </summary>
    /// <param name="item">The value to search for.</param>
    /// <param name="lower">Optional inclusive lower bound of the search range.</param>
    /// <param name="upper">Optional exclusive upper bound of the search range.</param>
    public int UpperBound(T item, int? lower = null, int? upper = null)
    {
        // Cannot rely on List<T>.BinarySearch because it may return any matching
        // index when duplicates exist. LowerBound must return the first element
        // >= item, so we perform the binary search manually.

        var low = lower ?? 0;
        var high = upper ?? _count;

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
    /// Returns the index of the first element that compares greater than or equal to the specified item.
    /// </summary>
    /// <param name="item">The value to search for.</param>
    /// <param name="lower">Optional inclusive lower bound of the search range.</param>
    /// <param name="upper">Optional exclusive upper bound of the search range.</param>
    public int LowerBound(T item, int? lower = null, int? upper = null)
    {
        // Cannot rely on List<T>.BinarySearch because it may return any matching
        // index when duplicates exist. LowerBound must return the first element
        // >= item, so we perform the binary search manually.

        var low = lower ?? 0;
        var high = upper ?? _count;

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
        var start = LowerBound(min);

        for (var i = start; i < _count; i++)
        {
            var item = _items[i];
            if (_comparer.Compare(item, max) > 0)
                yield break;

            yield return item;
        }
    }

    /// <summary>
    /// Removes the first element whose sort key compares equal to the specified item.
    /// </summary>
    /// <remarks>
    /// Equality is determined by the <see cref="IComparer{T}"/> used by the collection.<br/>
    /// </remarks>
    public bool RemoveOne(T item)
    {
        var index = IndexOf(item);

        if (index < 0)
            return false;

        RemoveAt(index);
        return true;
    }

    /// <summary>
    /// Removes all elements that are equal to the specified item.
    /// </summary>
    /// <remarks>
    /// Equality is determined by the <see cref="IComparer{T}"/> used by the collection.<br/>
    /// </remarks>
    public int RemoveAll(T item)
    {
        var start = IndexOf(item);
        if (start < 0)
            return 0;

        var end = LastIndexOf(item);
        var count = end - start + 1;

        RemoveRange(start, count);

        return count;
    }

    /// <summary>
    /// This method removes all items which matches the predicate.<br/>
    /// The complexity is O(n).
    /// </summary>
    public int RemoveAll(Predicate<T> match)
    {
        Check.NotNull(match);

        var freeIndex = 0;   // the first free slot in items array

        // Find the first item which needs to be removed.
        while (freeIndex < _count && !match(_items[freeIndex]))
            freeIndex++;

        if (freeIndex >= _count)
            return 0;

        var current = freeIndex + 1;
        while (current < _count)
        {
            // Find the first item which needs to be kept.
            while (current < _count && match(_items[current])) current++;

            if (current < _count)
            {
                // copy item to the free slot.
                _items[freeIndex++] = _items[current++];
            }
        }

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            Array.Clear(_items, freeIndex, _count - freeIndex); // Clear the elements so that the gc can reclaim the references.
        }

        var result = _count - freeIndex;
        _count = freeIndex;
        _version++;
        return result;
    }

    private void RemoveRange(int index, int count)
    {
        if (count <= 0)
            return;

        _count -= count;

        if (index < _count)
        {
            Array.Copy(_items, index + count, _items, index, _count - index);
        }

        ++_version;

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            Array.Clear(_items, _count, count);
        }
    }

    /// <summary>
    /// Returns the number of elements that are equal to the specified item.
    /// </summary>
    /// <remarks>
    /// Equality is determined by the <see cref="IComparer{T}"/> used by the collection.<br/>
    /// </remarks>
    public int CountOf(T item)
    {
        var end = LastIndexOf(item);
        if (end < 0)
            return 0;

        return end - IndexOf(item) + 1;
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
        var start = LowerBound(min);
        var end = UpperBound(max);
        var count = end - start;

        RemoveRange(start, count);

        return count;
    }
}
