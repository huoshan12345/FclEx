namespace FclEx.Extensions;

[SuppressMessage("ReSharper", "MoveToExtensionBlock")]
public static class ListExtensions
{
    public static void Remove<T>(this IList<T> list, Func<T, bool> filter)
    {
        Check.NotNull(list);
        Check.NotNull(filter);

        for (var i = list.Count - 1; i >= 0; --i)
        {
            var item = list[i];
            if (filter(item))
            {
                list.RemoveAt(i);
            }
        }
    }

    public static void Swap<T>(this IList<T> list, int left, int right)
    {
        (list[left], list[right]) = (list[right], list[left]);
    }

    /// <summary>
    /// Performs a stable sort using the specified comparer.
    /// </summary>
    /// <remarks>
    /// <see cref="List{T}.Sort(IComparer{T})"/> is not guaranteed to be stable, so this method
    /// preserves the relative order of elements that compare equal.
    /// </remarks>
    public static void StableSort<T>(this IList<T> list, int index, int count, IComparer<T>? comparer = null)
    {
        Check.NotNull(list);

        if ((uint)index > (uint)list.Count)
            throw new ArgumentOutOfRangeException(nameof(index), index, "The index must be between zero and Count.");

        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), count, "The count must be non-negative.");

        if (count > list.Count - index)
            throw new ArgumentException("The index and count must refer to a valid range in the list.", nameof(count));

        if (count <= 1)
            return;

        comparer ??= Comparer<T>.Default;

        var indexes = new int[count];
        var items = new T[count];

        for (var i = 0; i < count; i++)
        {
            indexes[i] = i;
            items[i] = list[index + i];
        }

        Array.Sort(indexes, (a, b) =>
        {
            var c = comparer.Compare(items[a], items[b]);
            return c != 0 ? c : a.CompareTo(b);
        });

        for (var i = 0; i < count; i++)
        {
            list[index + i] = items[indexes[i]];
        }
    }

    public static void StableSort<T>(this IList<T> list, IComparer<T>? comparer = null)
    {
        Check.NotNull(list);
        list.StableSort(0, list.Count, comparer);
    }

    public static IList<T>? TrySet<T>(this IList<T>? list, int index, T value)
    {
        if (list != null && 0 <= index && index < list.Count)
            list[index] = value;
        return list;
    }

    public static ReadOnlySpan<T> AsReadOnlySpan<T>(this List<T>? list)
    {
        return CollectionsMarshal.AsSpan(list);
    }

    public static void AddSorted<T>(this List<T> list, T value, IComparer<T>? comparer = null)
    {
        var x = list.BinarySearch(value, comparer);
        list.Insert((x >= 0) ? x : ~x, value);
    }

    /// <summary>
    /// Moves the element at the specified source index to the specified destination index.
    /// </summary>
    /// <remarks>
    /// This method preserves the relative order of all other elements.<br/>
    /// For example, moving index 1 to index 3 in [A, B, C, D, E] produces [A, C, D, B, E].
    /// </remarks>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list whose element should be moved.</param>
    /// <param name="sourceIndex">The current index of the element to move.</param>
    /// <param name="destinationIndex">The target index to move the element to.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="list"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="sourceIndex"/> or <paramref name="destinationIndex"/> is outside the valid index range.
    /// </exception>
    public static void MoveAt<T>(this IList<T> list, int sourceIndex, int destinationIndex)
    {
        Check.NotNull(list);
        Check.Between(sourceIndex, 0, list.Count - 1);
        Check.Between(destinationIndex, 0, list.Count - 1);

        if (sourceIndex == destinationIndex)
            return;

        var item = list[sourceIndex];

        if (sourceIndex < destinationIndex)
        {
            for (var i = sourceIndex; i < destinationIndex; i++)
            {
                list[i] = list[i + 1];
            }
        }
        else
        {
            for (var i = sourceIndex; i > destinationIndex; i--)
            {
                list[i] = list[i - 1];
            }
        }

        list[destinationIndex] = item;
    }

#if !NET5_0_OR_GREATER
    /// <summary>
    /// Returns a read-only <see cref="ReadOnlyCollection{T}"/> wrapper
    /// for the specified list.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="list">The list to wrap.</param>
    /// <returns>An object that acts as a read-only wrapper around the current <see cref="IList{T}"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="list"/> is null.</exception>
    public static ReadOnlyCollection<T> AsReadOnly<T>(this IList<T> list) => new(list);
#endif

    extension<T>(List<T>)
    {
        public static List<T> operator +(List<T> list, List<T> other)
        {
            var count = list.Count + other.Count;
            var result = new List<T>(count);
            result.AddRange(list);
            result.AddRange(other);
            return result;
        }

        public static List<T> operator +(List<T> list, T item)
        {
            var result = new List<T>(list.Count + 1);
            result.AddRange(list);
            result.Add(item);
            return result;
        }
    }

    extension<T>(List<T> list)
    {
        public void operator +=(IEnumerable<T> other)
        {
            list.AddRange(other);
        }

        public void operator +=(T item)
        {
            list.Add(item);
        }
    }
}
