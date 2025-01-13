using static System.Threading.Tasks.TaskContinuationOptions;

namespace FclEx.Extensions;

partial class TaskExtensions
{
    private const TaskContinuationOptions ThenOptions = OnlyOnRanToCompletion | ExecuteSynchronously;

    public static Task<TResult> Then<TResult>(this Task task, Func<Task<TResult>> action)
    {
        Check.NotNull(task);
        Check.NotNull(action);
        return task.ContinueWith(t => action(), ThenOptions).Unwrap();
    }

    public static Task Then(this Task task, Func<Task> action)
    {
        Check.NotNull(task);
        Check.NotNull(action);
        return task.ContinueWith(t => action(), ThenOptions).Unwrap();
    }

    public static Task When(this Task task, bool condition, Func<Task> action)
    {
        Check.NotNull(task);
        Check.NotNull(action);
        return task.ContinueWith(t => condition ? action() : t, ThenOptions).Unwrap();
    }

    public static Task Then(this Task task, Action action)
    {
        return task.Then(() =>
        {
            action();
            return Task.CompletedTask;
        });
    }

    public static Task<TResult> Then<TResult>(this Task task, Func<TResult> action)
    {
        return task.Then(() => action().ToTask());
    }

    public static Task<TResult> Then<TResult>(this Task task, TResult result)
    {
        return task.Then(() => result.ToTask());
    }

    public static Task<TResult> Then<T, TResult>(this Task<T> task, Func<T, Task<TResult>> action)
    {
        Check.NotNull(task);
        Check.NotNull(action);
        return task.ContinueWith(t => action(t.Result), ThenOptions).Unwrap();
    }

    public static Task<TResult> Then<T, TResult>(this Task<T> task, Func<T, TResult> action)
    {
        Check.NotNull(task);
        Check.NotNull(action);
        return task.ContinueWith(t => action(t.Result).ToTask(), ThenOptions).Unwrap();
    }

    public static Task<T> Then<T>(this Task<T> task, Action<T> action)
    {
        Check.NotNull(task);
        Check.NotNull(action);
        return task.ContinueWith(t =>
        {
            action(t.Result);
            return t;
        }, ThenOptions).Unwrap();
    }

    public static Task<T> When<T>(this Task<T> task, Func<T, bool> condition, Action<T> action)
    {
        return task.Then(t =>
        {
            if (condition(t))
            {
                action(t);
            }
        });
    }

    public static Task<T> Then<T>(this Task<T> task, Func<T, Task> action)
    {
        Check.NotNull(task);
        Check.NotNull(action);
        return task.ContinueWith(t => action(t.Result).ContinueWith(_ => t.Result), ThenOptions).Unwrap();
    }

    public static Task<T> When<T>(this Task<T> task, Func<T, bool> condition, Func<T, Task> action)
    {
        return task.Then(t => condition(t) ? action(t) : Task.CompletedTask);
    }

    public static Task Catch(this Task task, Func<Exception, Task> action)
    {
        Check.NotNull(task);
        Check.NotNull(action);
        return task.ContinueWith(t => t switch
        {
            { IsFaulted: true, Exception: { } ex } => action(ex.GetBaseException()),
            { IsCanceled: true } => action(new TaskCanceledException(t)),
            _ => Task.CompletedTask,
        }, ExecuteSynchronously).Unwrap();
    }

    public static Task<T> Catch<T>(this Task<T> task, Func<Exception, Task<T>> action)
    {
        Check.NotNull(task);
        Check.NotNull(action);

        return task.ContinueWith(t => t switch
        {
            { IsFaulted: true, Exception: { } ex } => action(ex.GetBaseException()),
            { IsCanceled: true } => action(new TaskCanceledException(t)),
            _ => Task.FromResult(t.Result),
        }, ExecuteSynchronously).Unwrap();
    }
}