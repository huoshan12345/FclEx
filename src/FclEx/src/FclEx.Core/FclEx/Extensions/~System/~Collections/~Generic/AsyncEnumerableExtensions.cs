namespace FclEx.Extensions;

public static class AsyncEnumerableExtensions
{
    public static async Task<List<TSource>> ToListAsync<TSource>(this IAsyncEnumerable<TSource> source)
    {
        var list = new List<TSource>();
        await foreach (var item in source)
        {
            list.Add(item);
        }
        return list;
    }

    public static async Task<TSource[]> ToArrayAsync<TSource>(this IAsyncEnumerable<TSource> source)
    {
        var list = await source.ToListAsync();
        return list.AsSpan().ToArray();
    }
}