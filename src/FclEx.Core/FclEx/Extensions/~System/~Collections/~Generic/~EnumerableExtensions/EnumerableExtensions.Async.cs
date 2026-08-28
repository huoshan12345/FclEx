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
}
