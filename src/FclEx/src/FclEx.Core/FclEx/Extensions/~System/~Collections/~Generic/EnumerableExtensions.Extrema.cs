namespace FclEx.Extensions;

[SuppressMessage("ReSharper", "ConvertIfStatementToSwitchStatement")]
partial class EnumerableExtensions
{
    public static (IReadOnlyList<T> Items, int TotalCount) ExtremaBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey?> keySelector, bool maxima, IComparer<TKey>? comparer = null)
    {
        Check.NotNull(source);
        Check.NotNull(keySelector);
        comparer ??= Comparer<TKey>.Default;

        using var e = source.GetEnumerator();

        if (e.MoveNext() == false)
            return ([], 0);

        var total = 1;
        var value = e.Current;
        var key = keySelector(value);
        var items = new List<T> { value };

        Predicate<int> predicate = maxima
            ? m => m > 0
            : m => m < 0;

        while (e.MoveNext())
        {
            total++;

            var nextValue = e.Current;
            var nextKey = keySelector(nextValue);
            var compare = comparer.Compare(key!, nextKey!);

            if (compare == 0)
            {
                items.Add(nextValue);
            }
            else if (predicate(compare))
            {
                key = nextKey;
                items = [nextValue];
            }
        }

        return (items, total);
    }

    public static (IReadOnlyList<T> Items, int TotalCount) MinimaBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey?> keySelector, IComparer<TKey>? comparer = null)
    {
        return source.ExtremaBy(keySelector, false, comparer);
    }

    public static (IReadOnlyList<T> Items, int TotalCount) MaximaBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey?> keySelector, IComparer<TKey>? comparer = null)
    {
        return source.ExtremaBy(keySelector, true, comparer);
    }
}