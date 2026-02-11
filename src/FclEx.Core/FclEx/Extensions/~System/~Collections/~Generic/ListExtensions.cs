namespace FclEx.Extensions;

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

    public static T[] AsArray<T>(this List<T>? list)
    {
        return list.AsSpan().ToArray();
    }

    public static void AddSorted<T>(this List<T> list, T value, IComparer<T>? comparer = null)
    {
        var x = list.BinarySearch(value, comparer);
        list.Insert((x >= 0) ? x : ~x, value);
    }
}