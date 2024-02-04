namespace FclEx.Extensions;

public static partial class TaskExtensions
{
    public static Task WhenAll(this IEnumerable<Task> tasks)
    {
        return Task.WhenAll(tasks);
    }

    public static Task<T[]> WhenAll<T>(this IEnumerable<Task<T>> tasks)
    {
        return Task.WhenAll(tasks);
    }

    public static ConfiguredTaskAwaitable IgnoreSyncContext(this Task task)
    {
        return task.ConfigureAwait(false);
    }

    public static ConfiguredTaskAwaitable<T> IgnoreSyncContext<T>(this Task<T> task)
    {
        return task.ConfigureAwait(false);
    }

    public static Task<T> ToTask<T>(this T obj) => Task.FromResult(obj);

    public static ValueTask<T> ToValueTask<T>(this Task<T> task) => new(task);

    private static readonly Task<Unit> TaskUnit = Task.FromResult(Unit.Default);
    public static Task<Unit> ToTaskUnit(this Task task) => TaskUnit;
}