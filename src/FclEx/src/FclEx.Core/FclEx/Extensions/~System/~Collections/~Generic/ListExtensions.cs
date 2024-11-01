using FclEx.Accessors;

namespace FclEx.Extensions;

public static class ListExtensions
{
    private static readonly Lazy<Random> _random = new(() => new Random());

    public static void RemoveAll<T>(this IList<T> list, Func<T, bool> filter)
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

    public static void Shuffle<T>(this IList<T> list, Random? random = null)
    {
        Check.NotNull(list);
        var r = random ?? _random.Value;
        for (var i = list.Count - 1; i > 0; --i)
        {
            var randomIndex = r.Next(i + 1);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }

    public static T GetRandomly<T>(this IList<T> list, Random? random = null)
    {
        Check.NotEmpty(list);
        var r = random ?? _random.Value;
        var i = r.Next(0, list.Count - 1);
        return list[i];
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
}