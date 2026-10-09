namespace FclEx.Extensions;

public static partial class EnumerableExtensions
{
    /// <summary>
    /// Removes consecutive equal elements, retaining the first element of each run.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="comparer">
    /// The comparer used to test element equality, or <see langword="null"/> to use
    /// <see cref="EqualityComparer{T}.Default"/>.
    /// </param>
    /// <returns>
    /// A sequence containing the first element of each consecutive run of equal elements,
    /// in source order. Equal elements separated by unequal elements are retained.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// Argument validation is immediate; enumeration is deferred. Each enumeration of the result
    /// enumerates the source once, using constant additional space. The source enumerator is disposed
    /// when enumeration completes, fails, or the result enumerator is disposed early.
    /// The comparer must obey the equality comparer contract, including transitivity.
    /// For example, <c>[1, 1, 2, 2, 2, 1]</c> produces <c>[1, 2, 1]</c>.
    /// </remarks>
    public static IEnumerable<T> DistinctAdjacent<T>(
        this IEnumerable<T> source,
        IEqualityComparer<T>? comparer = null)
    {
        Check.NotNull(source);
        return Iterator(source, comparer ?? EqualityComparer<T>.Default);

        static IEnumerable<T> Iterator(IEnumerable<T> source, IEqualityComparer<T> comparer)
        {
            using var enumerator = source.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                yield break;
            }

            var previous = enumerator.Current;
            yield return previous;

            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                if (!comparer.Equals(previous, current))
                {
                    yield return current;
                    previous = current;
                }
            }
        }
    }

    /// <summary>
    /// Removes consecutive elements with equal keys, retaining the first element of each run.
    /// </summary>
    /// <typeparam name="TSource">The type of elements in the sequence.</typeparam>
    /// <typeparam name="TKey">The type of keys used to compare elements.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="keySelector">The function that extracts a key from each element.</param>
    /// <param name="comparer">
    /// The comparer used to test key equality, or <see langword="null"/> to use
    /// <see cref="EqualityComparer{TKey}.Default"/>.
    /// </param>
    /// <returns>
    /// A sequence containing the first source element of each consecutive run of equal keys,
    /// in source order. Equal keys separated by unequal keys are retained.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="source"/> or <paramref name="keySelector"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// Argument validation is immediate; enumeration and key selection are deferred.
    /// Each enumeration of the result enumerates the source once, using constant additional space,
    /// and invokes <paramref name="keySelector"/> exactly once per visited source element.
    /// The source enumerator is disposed when enumeration completes, fails, or the result enumerator
    /// is disposed early. Exceptions from the selector or comparer propagate during enumeration.
    /// The comparer must obey the equality comparer contract, including transitivity.
    /// </remarks>
    public static IEnumerable<TSource> DistinctAdjacentBy<TSource, TKey>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        IEqualityComparer<TKey>? comparer = null)
    {
        Check.NotNull(source);
        Check.NotNull(keySelector);
        return Iterator(source, keySelector, comparer ?? EqualityComparer<TKey>.Default);

        static IEnumerable<TSource> Iterator(
            IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            IEqualityComparer<TKey> comparer)
        {
            using var enumerator = source.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                yield break;
            }

            var current = enumerator.Current;
            var previousKey = keySelector(current);
            yield return current;

            while (enumerator.MoveNext())
            {
                current = enumerator.Current;
                var currentKey = keySelector(current);
                if (!comparer.Equals(previousKey, currentKey))
                {
                    yield return current;
                    previousKey = currentKey;
                }
            }
        }
    }
}
