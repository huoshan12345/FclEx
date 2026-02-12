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
        Check.NotNull(list);
        Check.NotNegative(left);
        Check.NotNegative(right);
        Check.LessThan(left, list.Count);
        Check.LessThan(right, list.Count);

        (list[left], list[right]) = (list[right], list[left]);
    }

    public static IList<T>? TrySet<T>(this IList<T>? list, int index, T value)
    {
        if (list != null && 0 <= index && index < list.Count)
            list[index] = value;
        return list;
    }

    public static Span<T> AsSpan<T>(this List<T>? list)
    {
#if NETSTANDARD2_0
        return list is null ? default : ArrayAccessor<T>.ItemsAccessor(list);
#else
        return CollectionsMarshal.AsSpan(list);
#endif
    }

    public static void AddSorted<T>(this List<T> list, T value, IComparer<T>? comparer = null)
    {
        var x = list.BinarySearch(value, comparer);
        list.Insert((x >= 0) ? x : ~x, value);
    }

    extension<T>(List<T>)
    {
        public static List<T> operator +(List<T> list, List<T> other)
        {
            var result = new List<T>(list.Count + other.Count);
            var array = result.AsArray();
            list.AsArray().CopyTo(array);
            other.AsArray().CopyTo(array, list.Count);
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

        public T[] AsArray()
        {
            return ArrayAccessor<T>.ItemsAccessor(list);
        }
    }
}