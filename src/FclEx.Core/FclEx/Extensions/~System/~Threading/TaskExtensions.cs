namespace FclEx.Extensions;

public static partial class TaskExtensions
{
    public static bool IsSuccessful(this Task task)
    {
        return task is { IsFaulted: false, IsCanceled: false, Status: TaskStatus.RanToCompletion };
    }

    public static ConfiguredTaskAwaitable IgnoreSyncContext(this Task task)
    {
        return task.ConfigureAwait(false);
    }

    public static ConfiguredTaskAwaitable<T> IgnoreSyncContext<T>(this Task<T> task)
    {
        return task.ConfigureAwait(false);
    }

    public static ValueTask<T> ToValueTask<T>(this Task<T> task) => new(task);

    private static readonly Task<Unit> TaskUnit = Task.FromResult(Unit.Default);

    public static Task<Unit> ToTaskUnit(this Task task) => task.Then(() => TaskUnit);
}