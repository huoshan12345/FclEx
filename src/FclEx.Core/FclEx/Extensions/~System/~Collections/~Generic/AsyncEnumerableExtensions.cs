namespace FclEx.Extensions;

public static class AsyncEnumerableExtensions
{
#if !NET10_0_OR_GREATER
    /// <summary>Creates a list from an <see cref="IAsyncEnumerable{T}"/>.</summary>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">An <see cref="IEnumerable{T}"/> to create a list from.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A list that contains the elements from the input sequence.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
    public static ValueTask<List<TSource>> ToListAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default)
    {
        Check.NotNull(source);

        return Impl(source.WithCancellation(cancellationToken));

        static async ValueTask<List<TSource>> Impl(
            ConfiguredCancelableAsyncEnumerable<TSource> source)
        {
            List<TSource> list = [];
            await foreach (TSource element in source)
            {
                list.Add(element);
            }

            return list;
        }
    }
#endif

    /// <summary>
    /// Asynchronously materializes an enumerable into an array.
    /// </summary>
    /// <param name="source">The sequence to enumerate.</param>
    /// <param name="cancellationToken">The token passed to the asynchronous enumerator.</param>
    public static async ValueTask<T[]> ToArrayAsync<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken = default)
    {
        var list = await source.ToListAsync(cancellationToken).ConfigureAwait(false);
        return list.ToArray();
    }
}
