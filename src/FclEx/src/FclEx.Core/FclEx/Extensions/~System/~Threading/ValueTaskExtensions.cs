namespace FclEx.Extensions;

public static class ValueTaskExtensions
{
    public static Task WhenAll(this IEnumerable<ValueTask> tasks)
    {
        return tasks.Select(t => t.AsTask()).WhenAll();
    }

    public static Task<T[]> WhenAll<T>(this IEnumerable<ValueTask<T>> tasks)
    {
        return tasks.Select(t => t.AsTask()).WhenAll();
    }

    public static ValueTask<T> ToValueTask<T>(this T obj) => new(obj);

    public static ConfiguredValueTaskAwaitable<T> IgnoreSyncContext<T>(this ValueTask<T> task)
    {
        return task.ConfigureAwait(false);
    }

    public static ConfiguredValueTaskAwaitable IgnoreSyncContext(this ValueTask task)
    {
        return task.ConfigureAwait(false);
    }
}