namespace FclEx.Extensions;

public static partial class EnumerableExtensions
{
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public static IEnumerable<TResult> SelectMany<T, TResult>(this IEnumerable<T> source, Func<T, T, TResult> resultSelector)
    {
        return source.SelectMany(m => source, resultSelector);
    }

    public static IEnumerable<KeyValuePair<T1, T2>> AsKeyValue<T1, T2>(this IEnumerable<ValueTuple<T1, T2>> enumerable)
    {
        return enumerable.Select(m => m.ToKeyValuePair());
    }

    public static IEnumerable<ValueTuple<T1, T2>> AsTuple<T1, T2>(this IEnumerable<KeyValuePair<T1, T2>> enumerable)
    {
        return enumerable.Select(m => m.ToValueTuple());
    }

    public static IEnumerable<T3> Select<T1, T2, T3>(this IEnumerable<(T1, T2)> source, Func<T1, T2, int, T3> selector)
    {
        return source.Select((m, i) => selector(m.Item1, m.Item2, i));
    }

    public static IEnumerable<T> SelectIf<T>(this IEnumerable<T> enumerable, Func<T, T> selector, bool condition)
    {
        return condition
            ? enumerable.Select(selector)
            : enumerable;
    }

    public static IEnumerable<T> SelectIf<T>(this IEnumerable<T> enumerable, bool condition, Func<T, T> selector)
        => enumerable.SelectIf(selector, condition);

    public static IEnumerable<T> SelectIf<T>(this IEnumerable<T> enumerable, Func<T, int, T> selector, bool condition)
    {
        return condition
            ? enumerable.Select(selector)
            : enumerable;
    }

    public static IEnumerable<T> SelectIf<T>(this IEnumerable<T> enumerable, bool condition, Func<T, int, T> selector)
        => enumerable.SelectIf(selector, condition);
}
