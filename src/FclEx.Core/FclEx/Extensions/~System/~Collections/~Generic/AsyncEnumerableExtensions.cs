namespace FclEx.Extensions;

public static class AsyncEnumerableExtensions
{
    public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source)
    {
        var list = new List<T>();
        await foreach (var item in source)
        {
            list.Add(item);
        }
        return list;
    }

    public static async Task<T[]> ToArrayAsync<T>(this IAsyncEnumerable<T> source)
    {
        var list = await source.ToListAsync();
        return list.AsReadOnlySpan().ToArray();
    }
}