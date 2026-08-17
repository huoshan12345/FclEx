namespace FclEx.Extensions;

public static class AsyncEnumerableExtensions
{
    /// <summary>
    /// Asynchronously materializes an enumerable into a list.
    /// </summary>
    /// <param name="source">The sequence to enumerate.</param>
    /// <param name="cancellationToken">The token passed to the asynchronous enumerator.</param>
    public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken = default)
    {
        var list = new List<T>();
        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            list.Add(item);
        }
        return list;
    }

    /// <summary>
    /// Asynchronously materializes an enumerable into an array.
    /// </summary>
    /// <param name="source">The sequence to enumerate.</param>
    /// <param name="cancellationToken">The token passed to the asynchronous enumerator.</param>
    public static async Task<T[]> ToArrayAsync<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken = default)
    {
        var list = await source.ToListAsync(cancellationToken).ConfigureAwait(false);
        return list.AsReadOnlySpan().ToArray();
    }
}
