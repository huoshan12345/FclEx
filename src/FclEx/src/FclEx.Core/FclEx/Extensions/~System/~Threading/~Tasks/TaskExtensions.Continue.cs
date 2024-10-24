namespace FclEx.Extensions;

partial class TaskExtensions
{
    private static void HandleFaultedOrCanceled(this Task task)
    {
        if (task.Exception is { } ex)
            ex.GetBaseException().ReThrow();

        if (task.IsCanceled)
            throw new TaskCanceledException(task);
    }

    public static Task Continue(this Task task, Func<Task> next)
    {
        return task.ContinueWith(m =>
        {
            m.HandleFaultedOrCanceled();
            return next();
        }).Unwrap();
    }

    public static Task Continue(this Task task, Action next)
    {
        return task.Continue(() =>
        {
            next();
            return Task.CompletedTask;
        });
    }

    public static Task<TNext> Continue<TNext>(this Task task, Func<Task<TNext>> next)
    {
        return task.ContinueWith(m =>
        {
            m.HandleFaultedOrCanceled();
            return next();
        }).Unwrap();
    }

    public static Task<TNext> Continue<TNext>(this Task task, Func<TNext> next)
    {
        return task.Continue(() => next().ToTask());
    }

    public static Task<TNext> Continue<T, TNext>(this Task<T> task, Func<T, Task<TNext>> next)
    {
        return task.ContinueWith(m =>
        {
            m.HandleFaultedOrCanceled();
            return next(m.Result);
        }).Unwrap();
    }

    public static Task<TNext> Continue<T, TNext>(this Task<T> task, Func<T, TNext> next)
    {
        return task.Continue(t => next(t).ToTask());
    }

    public static Task<T> Do<T>(this Task<T> task, Func<T, Task> action)
    {
        return task.Continue(async m =>
        {
            await action(m);
            return m;
        });
    }

    public static Task<T> Do<T>(this Task<T> task, Action<T> action)
    {
        return task.Do<T>(t =>
        {
            action(t);
            return Task.CompletedTask;
        });
    }

    public static Task<T> Do<T>(this Task<T> task, Func<T, bool> condition, Action<T> action)
    {
        return task.Do(m =>
        {
            if (condition(m))
                action(m);
        });
    }

    public static Task<T> Do<T>(this Task<T> task, Func<T, bool> condition, Func<T, Task> action)
    {
        return task.Do(m =>
        {
            if (condition(m))
                action(m);
        });
    }
}