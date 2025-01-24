using static System.Threading.Tasks.TaskContinuationOptions;

namespace FclEx.Extensions;

partial class TaskExtensions
{
    public static Task<T> Then<T>(this Task task, Func<Task<T>> action)
    {
        Check.NotNull(task);
        Check.NotNull(action);

        return task.ContinueWith(t => t switch
        {
            { IsFaulted: true, Exception: { } ex } => new TaskCompletionSource<T>().Exception(ex).Task,
            { IsCanceled: true } => throw new TaskCanceledException(t),
            _ => action(),
        }, ExecuteSynchronously).Unwrap();
    }

    public static Task Then(this Task task, Func<Task> action)
    {
        return task.Then(() => action().ToTaskUnit());
    }

    public static Task When(this Task task, bool condition, Func<Task> action)
    {
        return task.Then(() => condition ? action() : task);
    }

    public static Task Then(this Task task, Action action)
    {
        return task.Then(() =>
        {
            action();
            return Task.CompletedTask;
        });
    }

    public static Task<T> Then<T>(this Task task, Func<T> action)
    {
        return task.Then(() => action().ToTask());
    }

    public static Task<T> Then<T>(this Task task, T result)
    {
        return task.Then(() => result.ToTask());
    }

    public static Task<TResult> Then<T, TResult>(this Task<T> task, Func<T, Task<TResult>> action)
    {
        Check.NotNull(task);
        Check.NotNull(action);

        return task.ContinueWith(t => t switch
        {
            { IsFaulted: true, Exception: { } ex } => new TaskCompletionSource<TResult>().Exception(ex).Task,
            { IsCanceled: true } => throw new TaskCanceledException(t),
            _ => action(t.Result),
        }, ExecuteSynchronously).Unwrap();
    }

    public static Task<TResult> Then<T, TResult>(this Task<T> task, Func<T, TResult> action)
    {
        Check.NotNull(task);
        Check.NotNull(action);

        return task.Then(t => action(t).ToTask());
    }

    public static Task<T> Then<T>(this Task<T> task, Action<T> action)
    {
        Check.NotNull(task);
        Check.NotNull(action);

        return task.Then(t =>
        {
            action(t);
            return task;
        });
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
        return task.Then(t => action(t).Then(() => task));
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
            { IsCanceled: true } => action(new TaskCanceledException(t).SetStackTrace()),
            _ => Task.FromResult(t.Result),
        }, ExecuteSynchronously).Unwrap();
    }
}