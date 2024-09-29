namespace FclEx.Extensions;

public readonly record struct IndexedItem<T>(int Index, T Item, bool IsFirst, bool IsLast);

[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
public static partial class EnumerableExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this IEnumerable<T>? source) => source is null || source.Any() == false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEmpty<T>(this IEnumerable<T> source) => source.Any() == false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNotEmpty<T>([NotNullWhen(true)] this IEnumerable<T>? enumerable) => enumerable.IsNullOrEmpty() == false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? source) => source ?? [];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string JoinWith<T>(this IEnumerable<T> enumerable, string? separator) => string.Join(separator, enumerable);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> NotNull<T>(this IEnumerable<T?> enumerable) => enumerable.Where(m => m is not null)!;

    public static IEnumerable<T> Not<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate) => enumerable.Where(m => predicate(m) == false);

    public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate, bool condition)
    {
        return condition ? enumerable.Where(predicate) : enumerable;
    }

    public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> enumerable, Func<T, int, bool> predicate, bool condition)
    {
        return condition ? enumerable.Where(predicate) : enumerable;
    }

    public static IEnumerable<T> TryTake<T>(this IEnumerable<T> enumerable, int? count)
    {
        return count is { } c ? enumerable.Take(c) : enumerable;
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
        foreach (var (i, b) in bits.Index())
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

    public static IEnumerable<(int, T)> Index<T>(this IEnumerable<T> enumerable)
    {
        var i = 0;
        foreach (var item in enumerable)
        {
            yield return (i++, item);
        }
    }

    public static IEnumerable<IndexedItem<T>> IndexExt<T>(this IEnumerable<T> enumerable)
    {
        ArgumentNullException.ThrowIfNull(enumerable);

        // we separate the null check from the method body with yield, otherwise the null check will not be executed until start enumerating.
        // see details in https://stackoverflow.com/questions/42149895/method-having-yield-return-is-not-throwing-exception
        return WithIndexBody(enumerable);

        static IEnumerable<IndexedItem<T>> WithIndexBody(IEnumerable<T> enumerable)
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
                yield return new(i, current, i == 0, false);
                current = enumerator.Current;
                ++i;
            }

            yield return new(i, current, i == 0, true);
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

    public static async IAsyncEnumerable<TResult> SelectAsync<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, Task<TResult>> selector)
    {
        foreach (var item in source)
        {
            yield return await selector(item);
        }
    }

    public static IEnumerable<T> Do<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
        {
            action(item);
            yield return item;
        }
    }

    public static async IAsyncEnumerable<T> DoAsync<T>(this IEnumerable<T> source, Func<T, Task> action)
    {
        foreach (var item in source)
        {
            await action(item);
            yield return item;
        }
    }

    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
        {
            action(item);
        }
    }

    public static void ForEach<T>(this IEnumerable<T> source, Action<int, T> action)
    {
        var i = 0;
        foreach (var item in source)
        {
            action(i++, item);
        }
    }

    public static async Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> action)
    {
        foreach (var item in source)
        {
            await action(item);
        }
    }

    public static async Task ForEachAsync<T>(this IEnumerable<T> source, Func<int, T, Task> action)
    {
        var i = 0;
        foreach (var item in source)
        {
            await action(i++, item);
        }
    }

    public static IEnumerable<KeyValuePair<TKey, TValue>> EnumerateMany<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, IReadOnlyCollection<TValue>>> enumerable)
    {
        foreach (var (key, values) in enumerable)
        {
            foreach (var value in values)
            {
                yield return new(key, value);
            }
        }
    }

    public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> list, int parts)
    {
        var i = 0;
        var splits = from item in list
                     group item by i++ % parts into part
                     select part;
        return splits;
    }

#if NET7_0_OR_GREATER
    public static T Sum<T>(this IEnumerable<T> enumerable) where T : IAdditionOperators<T, T, T>, IAdditiveIdentity<T, T>
    {
        return enumerable.Aggregate(T.AdditiveIdentity, (current, item) => current + item);
    }
#endif
}