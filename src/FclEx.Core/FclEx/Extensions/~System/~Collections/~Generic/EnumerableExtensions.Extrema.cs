namespace FclEx.Extensions;

[SuppressMessage("ReSharper", "ConvertIfStatementToSwitchStatement")]
partial class EnumerableExtensions
{
    /// <summary>
    /// Finds the extrema (minimum or maximum) elements in a sequence based on a specified key selector function.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <typeparam name="TKey">The type of the key used for comparison.</typeparam>
    /// <param name="source">The sequence of elements to evaluate.</param>
    /// <param name="keySelector">A function that extracts the key from each element for comparison.</param>
    /// <param name="maxima">If <see langword="true"/>, finds the maximum elements; if <see langword="false"/>, finds the minimum elements.</param>
    /// <param name="comparer">An optional comparer to compare the keys. If <see langword="null"/>, the default comparer for <typeparamref name="TKey"/> is used.</param>
    /// <returns>A tuple containing a list of the extrema elements and the total count of elements processed in the source sequence.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> or <paramref name="keySelector"/> is <see langword="null"/>.</exception>
    public static (List<T> Items, int TotalCount) ExtremaBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, bool maxima, IComparer<TKey>? comparer = null)
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
            ? m => m < 0
            : m => m > 0;

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

    /// <summary>
    /// Finds the minimum elements in a sequence based on a specified key selector function.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <typeparam name="TKey">The type of the key used for comparison.</typeparam>
    /// <param name="source">The sequence of elements to evaluate.</param>
    /// <param name="keySelector">A function that extracts the key from each element for comparison.</param>
    /// <param name="comparer">An optional comparer to compare the keys. If <see langword="null"/>, the default comparer for <typeparamref name="TKey"/> is used.</param>
    /// <returns>A tuple containing a list of the minimum elements and the total count of elements processed in the source sequence.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> or <paramref name="keySelector"/> is <see langword="null"/>.</exception>
    public static (List<T> Items, int TotalCount) MinimaBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, IComparer<TKey>? comparer = null)
    {
        return source.ExtremaBy(keySelector, false, comparer);
    }

    /// <summary>
    /// Finds the maximum elements in a sequence based on a specified key selector function.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <typeparam name="TKey">The type of the key used for comparison.</typeparam>
    /// <param name="source">The sequence of elements to evaluate.</param>
    /// <param name="keySelector">A function that extracts the key from each element for comparison.</param>
    /// <param name="comparer">An optional comparer to compare the keys. If <see langword="null"/>, the default comparer for <typeparamref name="TKey"/> is used.</param>
    /// <returns>A tuple containing a list of the maximum elements and the total count of elements processed in the source sequence.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> or <paramref name="keySelector"/> is <see langword="null"/>.</exception>
    public static (List<T> Items, int TotalCount) MaximaBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, IComparer<TKey>? comparer = null)
    {
        return source.ExtremaBy(keySelector, true, comparer);
    }

    /// <summary>
    /// Finds the elements in the sequence that have the minimum and maximum values
    /// based on a specified key selector function and an optional comparer.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <typeparam name="TKey">The type of the key used for comparison.</typeparam>
    /// <param name="source">The sequence of elements to evaluate.</param>
    /// <param name="keySelector">A function that extracts the key from each element for comparison.</param>
    /// <param name="comparer">
    /// An optional comparer to compare the keys. If <see langword="null"/>, the default comparer for <typeparamref name="TKey"/> is used.
    /// </param>
    /// <returns>
    /// A tuple containing the elements with the minimum and maximum keys as determined by <paramref name="keySelector"/>.
    /// If the sequence is empty, returns <c>(null, null)</c> if <typeparamref name="T"/> is a reference type;
    /// otherwise, throws an <see cref="InvalidOperationException"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown if the sequence is empty and <typeparamref name="T"/> is a value type.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> or <paramref name="keySelector"/> is <see langword="null"/>.</exception>
    public static (T? Min, T? Max) MinMaxBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, IComparer<TKey>? comparer = null)
    {
        Check.NotNull(source);
        Check.NotNull(keySelector);
        comparer ??= Comparer<TKey>.Default;

        using var e = source.GetEnumerator();

        if (e.MoveNext() == false)
        {
            if (default(T) is null)
            {
                return (default, default);
            }
            else
            {
                throw new InvalidOperationException("Sequence contains no elements");
            }
        }

        var value = e.Current;
        var minKey = keySelector(value);
        var maxKey = minKey;
        var min = value;
        var max = value;

        while (e.MoveNext())
        {
            var nextValue = e.Current;
            var nextKey = keySelector(nextValue);
            if (comparer.Compare(minKey, nextKey) > 0)
            {
                minKey = nextKey;
                min = nextValue;
            }

            if (comparer.Compare(maxKey, nextKey) < 0)
            {
                maxKey = nextKey;
                max = nextValue;
            }
        }

        return (min, max);
    }
}