namespace FclEx.Extensions;

[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
public static partial class EnumerableExtensions
{
    public static TCollection AsCollection<T, TCollection>(this IEnumerable<T> source, Func<IEnumerable<T>, TCollection> factory)
    {
        return source switch
        {
            null => throw new ArgumentNullException(nameof(source)),
            TCollection col => col,
            _ => factory(source),
        };
    }

    public static ReadOnlyCollection<T> AsReadOnlyCollection<T>(this IEnumerable<T> source)
    {
        return source.AsCollection(m => new ReadOnlyCollection<T>(m.ToArray()));
    }

    public static ReadOnlyHashSet<T> AsReadOnlyHashSet<T>(this IEnumerable<T> source)
    {
        return source.AsCollection(m => new ReadOnlyHashSet<T>(m.ToHashSet()));
    }

    public static IList<T> AsIList<T>(this IEnumerable<T> source)
    {
        return source.AsCollection<T, IList<T>>(m => m.ToList());
    }

    public static ISet<T> AsISet<T>(this IEnumerable<T> source)
    {
        return source.AsCollection<T, ISet<T>>(m => m.ToHashSet());
    }

    public static ICollection<T> AsICollection<T>(this IEnumerable<T> source)
    {
        return source.AsCollection<T, ICollection<T>>(m => m.ToList());
    }

    public static IReadOnlyList<T> AsIReadOnlyList<T>(this IEnumerable<T> source)
    {
        return source.AsCollection<T, IReadOnlyList<T>>(m => m.ToList());
    }

    public static IReadOnlyCollection<T> AsIReadOnlyCollection<T>(this IEnumerable<T> source)
    {
        return source.AsCollection<T, IReadOnlyCollection<T>>(m => m.ToList());
    }

    public static List<T> AsList<T>(this IEnumerable<T> source)
    {
        return source.AsCollection<T, List<T>>(m => m.ToList());
    }

    public static T[] AsArray<T>(this IEnumerable<T> source)
    {
        return source.AsCollection<T, T[]>(m => m.ToArray());
    }

#if !NET5_0_OR_GREATER
    public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(
        this IEnumerable<KeyValuePair<TKey, TValue>> source,
        IEqualityComparer<TKey>? comparer = null)
        where TKey : notnull
    {
        return source.ToDictionary(m => m.Key, m => m.Value, comparer);
    }
#endif

    public static Dictionary<TKey, TValue> AsDictionary<TKey, TValue>(
        this IEnumerable<KeyValuePair<TKey, TValue>> source,
        IEqualityComparer<TKey>? comparer = null)
        where TKey : notnull
    {
        return source switch
        {
            null => throw new ArgumentNullException(nameof(source)),
            Dictionary<TKey, TValue> dic when Equals(comparer, dic.Comparer) => dic,
            _ => source.ToDictionary(comparer),
        };

        static bool Equals(IEqualityComparer<TKey>? x, IEqualityComparer<TKey>? y)
        {
            var defaultComparer = EqualityComparer<TKey>.Default;
            if (x is null && (y is null || defaultComparer.Equals(y)))
                return true;

            if (y is null && (x is null || defaultComparer.Equals(x)))
                return true;

            if (x is null || y is null)
                return false;

            return x.Equals(y);
        }
    }

    public static IReadOnlyDictionary<TKey, TValue> AsIReadOnlyDictionary<TKey, TValue>(
        this IEnumerable<KeyValuePair<TKey, TValue>> source,
        IEqualityComparer<TKey>? comparer = null)
        where TKey : notnull
    {
        return source.AsDictionary(comparer);
    }

    public static IDictionary<TKey, TValue> AsIDictionary<TKey, TValue>(
        this IEnumerable<KeyValuePair<TKey, TValue>> source,
        IEqualityComparer<TKey>? comparer = null)
        where TKey : notnull
    {
        return source.AsDictionary(comparer);
    }
}