namespace FclEx.Extensions;

public static partial class EnumerableExtensions
{
    public static async IAsyncEnumerable<TResult> SelectAsync<T, TResult>(this IEnumerable<T> source, Func<T, Task<TResult>> selector)
    {
        foreach (var item in source)
        {
            yield return await selector(item);
        }
    }

    public static async IAsyncEnumerable<T> DoAsync<T>(this IEnumerable<T> source, Func<T, Task> action)
    {
        foreach (var item in source)
        {
            await action(item);
            yield return item;
        }
    }

    public static async Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> action)
    {
        foreach (var item in source)
        {
            await action(item);
        }
    }

    public static async Task ForEachAsync<T>(this IEnumerable<T> source, Func<int, T, Task> action)
    {
        var i = 0;
        foreach (var item in source)
        {
            await action(i++, item);
        }
    }
}
