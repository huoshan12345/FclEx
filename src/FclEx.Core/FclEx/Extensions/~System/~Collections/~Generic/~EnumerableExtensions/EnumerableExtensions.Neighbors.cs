namespace FclEx.Extensions;

public static partial class EnumerableExtensions
{
    /// <summary>
    /// Enumerates the sequence while providing each element together with its immediate predecessor.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="enumerable">The source sequence.</param>
    /// <returns>
    /// A sequence of tuples where:
    /// <list type="bullet">
    /// <item><description><c>Item</c> is the current element.</description></item>
    /// <item><description><c>Previous</c> is the previous element, or <see langword="default"/> for the first element.</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// The source sequence is enumerated only once and evaluated lazily.
    /// </remarks>
    public static IEnumerable<(T Item, T? Previous)> WithPrevious<T>(this IEnumerable<T> enumerable)
    {
        Check.NotNull(enumerable);
        return WithPreviousIterator(enumerable);

        static IEnumerable<(T Item, T? Previous)> WithPreviousIterator(IEnumerable<T> enumerable)
        {
            var previous = default(T);
            foreach (var item in enumerable)
            {
                yield return (item, previous);
                previous = item;
            }
        }
    }

    /// <summary>
    /// Enumerates the sequence while providing each element together with its immediate successor.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="enumerable">The source sequence.</param>
    /// <returns>
    /// A sequence of tuples where:
    /// <list type="bullet">
    /// <item><description><c>Item</c> is the current element.</description></item>
    /// <item><description><c>Next</c> is the next element, or <see langword="default"/> for the last element.</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// The source sequence is enumerated only once and evaluated lazily.
    /// </remarks>
    public static IEnumerable<(T Item, T? Next)> WithNext<T>(this IEnumerable<T> enumerable)
    {
        Check.NotNull(enumerable);
        return WithNextIterator(enumerable);

        static IEnumerable<(T Item, T? Next)> WithNextIterator(IEnumerable<T> enumerable)
        {
            using var enumerator = enumerable.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                yield break;
            }

            var current = enumerator.Current;
            while (enumerator.MoveNext())
            {
                var next = enumerator.Current;
                yield return (current, next);
                current = next;
            }

            yield return (current, default);
        }
    }

    /// <summary>
    /// Enumerates the sequence while providing each element together with its
    /// immediate predecessor and successor.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="enumerable">The source sequence.</param>
    /// <returns>
    /// A sequence of tuples where:
    /// <list type="bullet">
    /// <item><description><c>Item</c> is the current element.</description></item>
    /// <item><description><c>Previous</c> is the previous element, or <see langword="default"/> for the first element.</description></item>
    /// <item><description><c>Next</c> is the next element, or <see langword="default"/> for the last element.</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// The source sequence is enumerated only once and evaluated lazily.
    /// </remarks>
    public static IEnumerable<(T Item, T? Previous, T? Next)> WithNeighbors<T>(this IEnumerable<T> enumerable)
    {
        Check.NotNull(enumerable);
        return WithNeighborsIterator(enumerable);

        static IEnumerable<(T Item, T? Previous, T? Next)> WithNeighborsIterator(IEnumerable<T> enumerable)
        {
            using var enumerator = enumerable.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                yield break;
            }

            var previous = default(T);
            var current = enumerator.Current;
            while (enumerator.MoveNext())
            {
                var next = enumerator.Current;
                yield return (current, previous, next);
                previous = current;
                current = next;
            }

            yield return (current, previous, default);
        }
    }
}
