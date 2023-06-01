using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace FclEx.Extensions;

[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
public static partial class EnumerableExtensions
{
    public static bool IsValid<T>([NotNullWhen(true)] this IEnumerable<T>? source)
    {
        return !source.IsNullOrEmpty();
    }

    public static bool IsEmpty<T>(this IEnumerable<T> source)
    {
        return !source.Any();
    }

    public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this IEnumerable<T>? source)
    {
        return source == null || !source.Any();
    }

    public static IEnumerable<T> Touch<T>(this IEnumerable<T>? source)
    {
        return source ?? Enumerable.Empty<T>();
    }

    public static string JoinWith<T>(this IEnumerable<T> strs, string separator)
    {
        return string.Join(separator, strs.Select(m => m?.ToString()));
    }

    public static IEnumerable<T> NotNull<T>(this IEnumerable<T?> col) where T : class
    {
        return col.Where(m => m != null)!;
    }

    public static IEnumerable<string> Valid(this IEnumerable<string?> col)
    {
        return col.Where(m => m.IsValid())!;
    }

    public static IEnumerable<T> NotNull<T>(this IEnumerable<T?> col) where T : struct
    {
        return col.Where(m => m != null).Select(m => m.Get());
    }

    public static IEnumerable<T> Not<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        return source.Where(m => !predicate(m));
    }

    public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> source, Func<T, bool> predicate, bool condition)
    {
        return condition ? source.Where(predicate) : source;
    }

    public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> source, Func<T, int, bool> predicate, bool condition)
    {
        return condition ? source.Where(predicate) : source;
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

    public static IEnumerable<T> Concat<T>(this T firstElement, IEnumerable<T>? secondSequence)
    {
        yield return firstElement;
        if (secondSequence == null)
        {
            yield break;
        }

        foreach (var item in secondSequence)
        {
            yield return item;
        }
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
}