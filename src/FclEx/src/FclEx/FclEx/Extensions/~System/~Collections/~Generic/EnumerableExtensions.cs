using System.Numerics;

namespace FclEx.Extensions;

[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
public static partial class EnumerableExtensions
{
    public static bool IsValid<T>([NotNullWhen(true)] this IEnumerable<T>? enumerable)
    {
        return !enumerable.IsNullOrEmpty();
    }

    public static bool IsEmpty<T>(this IEnumerable<T> enumerable)
    {
        return !enumerable.Any();
    }

    public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this IEnumerable<T>? source)
    {
        return source == null || !source.Any();
    }

    public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? source)
    {
        return source ?? Enumerable.Empty<T>();
    }

    public static string JoinWith<T>(this IEnumerable<T> enumerable, string? separator)
    {
        return string.Join(separator, enumerable);
    }

    public static IEnumerable<T> NotNull<T>(this IEnumerable<T?> enumerable) where T : class
    {
        return enumerable.Where(m => m != null)!;
    }

    public static IEnumerable<T> NotNull<T>(this IEnumerable<T?> enumerable) where T : struct
    {
        return enumerable.Where(m => m != null).Select(m => m.Get());
    }

    public static IEnumerable<T> Not<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
    {
        return enumerable.Where(m => !predicate(m));
    }

    public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate, bool condition)
    {
        return condition ? enumerable.Where(predicate) : enumerable;
    }

    public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> enumerable, Func<T, int, bool> predicate, bool condition)
    {
        return condition ? enumerable.Where(predicate) : enumerable;
    }

    public static IEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> source,
        Func<TSource, TSource, TResult> resultSelector)
    {
        return source.SelectMany(m => source, resultSelector);
    }

    public static IEnumerable<T> Except<T>(this IEnumerable<T> source, T item, IEqualityComparer<T>? comparer = null)
    {
        comparer ??= EqualityComparer<T>.Default;
        return source.Where(m => !comparer.Equals(m, item));
    }

    public static SortedSet<T> ToSortedSet<T>(this IEnumerable<T> enumerable, IComparer<T>? comparer = null)
    {
        return new SortedSet<T>(enumerable, comparer ?? Comparer<T>.Default);
    }

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public static TProp[] ToArrayByIndex<T, TProp>(this IEnumerable<T> enumerable, Func<T, int> indexSelector, Func<T, TProp> valueSelector)
    {
        Check.NotNull(enumerable);
        Check.NotNull(indexSelector);
        Check.NotNull(valueSelector);

        if (!enumerable.Any())
            return Array.Empty<TProp>();

        var max = enumerable.Max(indexSelector);
        var list = new TProp[max + 1];
        foreach (var item in enumerable)
        {
            var index = indexSelector(item);
            list[index] = valueSelector(item);
        }
        return list;
    }

    public static IEnumerable<KeyValuePair<T1, T2>> AsKeyValue<T1, T2>(this IEnumerable<ValueTuple<T1, T2>> enumerable)
    {
        return enumerable.Select(m => m.AsKeyValue());
    }

    public static IEnumerable<ValueTuple<T1, T2>> AsTuple<T1, T2>(this IEnumerable<KeyValuePair<T1, T2>> enumerable)
    {
        return enumerable.Select(m => m.AsTuple());
    }

    public static TResult? MaxOr<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector, TResult? defaultValue = default)
    {
        return source.Any() ? source.Max(selector) : defaultValue;
    }

    public static TSource? MaxOr<TSource>(this IEnumerable<TSource> source, TSource? defaultValue = default)
    {
        return source.Any() ? source.Max() : defaultValue;
    }

    public static TResult? MinOr<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector, TResult? defaultValue = default)
    {
        return source.Any() ? source.Min(selector) : defaultValue;
    }

    public static TSource? MinOr<TSource>(this IEnumerable<TSource> source, TSource? defaultValue = default)
    {
        return source.Any() ? source.Min() : defaultValue;
    }

    public static TimeSpan Average<TSource>(this IEnumerable<TSource> source, Func<TSource, TimeSpan> selector)
    {
        var ticks = (long)source.Select(m => selector(m).Ticks).Average();
        return TimeSpan.FromTicks(ticks);
    }

    public static TimeSpan Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, TimeSpan> selector)
    {
        var ticks = source.Select(m => selector(m).Ticks).Sum();
        return TimeSpan.FromTicks(ticks);
    }

    public static int BitsToInt(this IEnumerable<bool> bits)
    {
        var num = 0;
        foreach (var (b, i, _, _) in bits.IndexExt())
        {
            var bit = b ? 1 : 0;
            num &= (bit << i);
        }
        return num;
    }

    public static IEnumerable<(T1, T2)> CrossJoin<T1, T2>(this IEnumerable<T1> left, IEnumerable<T2> right)
    {
        return left.SelectMany(m => right, (t1, t2) => (t1, t2));
    }

    public static IEnumerable<T3> Select<T1, T2, T3>(this IEnumerable<(T1, T2)> source, Func<T1, T2, int, T3> selector)
    {
        return source.Select((m, i) => selector(m.Item1, m.Item2, i));
    }

    // Extension for MoreLinq.Index()
    public static IEnumerable<(T item, int index, bool isFirst, bool isLast)> IndexExt<T>(this IEnumerable<T> enumerable)
    {
        if (enumerable == null)
        {
            throw new ArgumentNullException(nameof(enumerable));
        }

        // we separate the null check from the method body with yield, otherwise the null check will not be executed until start enumerating.
        // see details in https://stackoverflow.com/questions/42149895/method-having-yield-return-is-not-throwing-exception
        return WithIndexBody(enumerable);

        static IEnumerable<(T item, int index, bool isFirst, bool isLast)> WithIndexBody(IEnumerable<T> enumerable)
        {
            using var enumerator = enumerable.GetEnumerator();

            if (!enumerator.MoveNext())
            {
                yield break;
            }

            var i = 0;
            var current = enumerator.Current;
            while (enumerator.MoveNext())
            {
                yield return (current, i, i == 0, false);
                current = enumerator.Current;
                ++i;
            }

            yield return (current, i, i == 0, true);
        }
    }

    public static bool AnyExt<T>(this IEnumerable<T> enumerable)
    {
        Check.NotNull(enumerable);

        if (enumerable is IReadOnlyCollection<T> collection)
        {
            return collection.Count > 0;
        }
        return enumerable.Any();
    }

    public static IEnumerable<T> Concat<T>(this IEnumerable<IEnumerable<T>> arrays)
    {
        return arrays.SelectMany(m => m);
    }

    public static IEnumerable<T> Concat<T>(this IEnumerable<T> source, IEnumerable<T>[] arrays)
    {
        return arrays.Prepend(source).Concat();
    }

    public static IEnumerable<(T Left, T2 Right)> SelectMany<T, T2>(this IEnumerable<T> left, IEnumerable<T2> right)
    {
        return left.SelectMany(_ => right, (x, y) => (x, y));
    }

    public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> enumerable, Func<TSource, TKey> keySelector, bool desc)
    {
        return desc
            ? enumerable.OrderByDescending(keySelector)
            : enumerable.OrderBy(keySelector);
    }

#if NET7_0_OR_GREATER
    public static T Sum<T>(this IEnumerable<T> enumerable) where T : IAdditionOperators<T, T, T>, IAdditiveIdentity<T, T>
    {
        return enumerable.Aggregate(T.AdditiveIdentity, (current, item) => current + item);
    }
#endif
}