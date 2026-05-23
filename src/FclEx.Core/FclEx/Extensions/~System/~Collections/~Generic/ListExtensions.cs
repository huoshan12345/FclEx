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

        if (list.Count == 0)
            return;

        Check.Between(index, 0, list.Count - 1);
        Check.Between(count, 0, list.Count - index);

        if (count == 0)
            return;

        comparer ??= Comparer<T>.Default;

        var indexes = new int[count];

        for (var i = 0; i < count; i++)
            indexes[i] = i;

        Array.Sort(indexes, (a, b) =>
        {
            var c = comparer.Compare(list[a], list[b]);
            return c != 0 ? c : a.CompareTo(b);
        });

        var temp = list.ToArray();

        for (var i = 0; i < count; i++)
        {
            list[i] = temp[indexes[i]];
        }
    }

    public static void StableSort<T>(this IList<T> list, IComparer<T>? comparer = null)
    {
        list.StableSort(0, list.Count, comparer);
    }

    public static IList<T>? TrySet<T>(this IList<T>? list, int index, T value)
    {
        if (list != null && 0 <= index && index < list.Count)
            list[index] = value;
        return list;
    }

    public static Span<T> AsSpan<T>(this List<T>? list)
    {
#if !NET5_0_OR_GREATER
        return list.IsNullOrEmpty()
            ? default
            : ListAccessor<T>.Items(list).AsSpan(0, list.Count);
#else
        return CollectionsMarshal.AsSpan(list);
#endif
    }

    public static void AddSorted<T>(this List<T> list, T value, IComparer<T>? comparer = null)
    {
        var x = list.BinarySearch(value, comparer);
        list.Insert((x >= 0) ? x : ~x, value);
    }

    public static T[] Items<T>(this List<T> list)
    {
        return ListAccessor<T>.Items(list);
    }

    public static void SetCount<T>(this List<T> list, int count)
    {
        Check.NotNegative(count);

        ref var version = ref ListAccessor<T>.Version(list);
        ++version;

        ref var size = ref ListAccessor<T>.Size(list);
        if (count > list.Capacity)
        {
            list.Capacity = count;
        }
        else if (count < size)
        {
            var items = ListAccessor<T>.Items(list);
            Array.Clear(items, count, size - count);
        }
        size = count;
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
            var array = result.Items();
            list.CopyTo(array);
            other.CopyTo(array, list.Count);
            result.SetCount(count);
            return result;
        }

        public static List<T> operator +(List<T> list, T item)
        {
            list.Add(item);
            return list;
        }
    }

    extension<T>(List<T> list)
    {
        public void operator +=(IEnumerable<T> other)
        {
            list.AddRange(other);
        }
    }
}