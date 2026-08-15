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

    public static ConfiguredValueTaskAwaitable<T> NoCapture<T>(this ValueTask<T> task)
    {
        return task.NoCapture();
    }

    public static ConfiguredValueTaskAwaitable NoCapture(this ValueTask task)
    {
        return task.NoCapture();
    }

    extension(ValueTask)
    {
#if !NET5_0_OR_GREATER
        public static ValueTask CompletedTask => default;

        public static ValueTask<TResult> FromResult<TResult>(TResult result) => new(result);
#endif
    }
}