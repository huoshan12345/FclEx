#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
using static System.Threading.Tasks.TaskContinuationOptions;

namespace FclEx.Extensions;

partial class TaskExtensions
{
    public static Task Then(this Task task, Func<Task> action)
    {
        return task.Then(() => action().ToTaskUnit());
    }

    public static Task Then(this Task task, Action action)
    {
        return task.Then(() =>
        {
            action();
            return Task.CompletedTask;
        });
    }

    public static async Task<T> Then<T>(this Task task, Func<Task<T>> action)
    {
        Check.NotNull(task);
        Check.NotNull(action);

        await task.ConfigureAwait(false);
        return await action().ConfigureAwait(false);
    }

    public static Task<T> Then<T>(this Task task, Func<T> action)
    {
        return task.Then(() => Task.FromResult(action()));
    }

    public static async Task<TResult> Then<T, TResult>(this Task<T> task, Func<T, Task<TResult>> action)
    {
        Check.NotNull(task);
        Check.NotNull(action);

        await task.ConfigureAwait(false);
        return await action(task.Result).ConfigureAwait(false);
    }

    public static Task<TResult> Then<T, TResult>(this Task<T> task, Func<T, TResult> action)
    {
        Check.NotNull(task);
        Check.NotNull(action);

        return task.Then(t => Task.FromResult(action(t)));
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

    public static Task<T> Then<T>(this Task<T> task, Func<T, Task> action)
    {
        Check.NotNull(task);
        Check.NotNull(action);
        return task.Then(t => action(t).Then(() => task));
    }

    public static Task<IOPair<TInput, TOutput>> ToIOPair<TInput, TOutput>(this Task<TOutput> task, TInput input)
    {
        return task.Then(m => IOPair.Create(input, m));
    }

    public static async Task Catch(this Task task, Func<Exception, Task> action)
    {
        Check.NotNull(task);
        Check.NotNull(action);

        try
        {
            await task.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await action(ex).ConfigureAwait(false);
        }
    }

    public static Task Catch(this Task task, Action<Exception> action)
    {
        return task.Catch(ex =>
        {
            action(ex);
            return Task.CompletedTask;
        });
    }

    public static async Task<T> Catch<T>(this Task<T> task, Func<Exception, Task<T>> action)
    {
        Check.NotNull(task);
        Check.NotNull(action);

        try
        {
            return await task.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return await action(ex).ConfigureAwait(false);
        }
    }

    public static Task<T> Catch<T>(this Task<T> task, Action<Exception> action)
    {
        return task.Catch(ex =>
        {
            action(ex);
            return Task.FromResult(default(T)!);
        });
    }

    public static async Task<T> Finally<T>(this Task<T> task, Func<Task> action)
    {
        Check.NotNull(task);
        Check.NotNull(action);

        try
        {
            return await task.ConfigureAwait(false);
        }
        finally
        {
            await action().ConfigureAwait(false);
        }
    }

    public static Task<T> Finally<T>(this Task<T> task, Action action)
    {
        return task.Finally(() =>
        {
            action();
            return Task.CompletedTask;
        });
    }

    public static async Task Finally(this Task task, Func<Task> action)
    {
        Check.NotNull(task);
        Check.NotNull(action);

        try
        {
            await task.ConfigureAwait(false);
        }
        finally
        {
            await action().ConfigureAwait(false);
        }
    }

    public static Task Finally(this Task task, Action action)
    {
        return task.Finally(() =>
        {
            action();
            return Task.CompletedTask;
        });
    }


    public static Task ThenIf(this Task task, Func<Task> action, bool condition)
    {
        return task.Then(() => condition ? action() : task);
    }

    public static Task<T> ThenIf<T>(this Task<T> task, Action<T> action, Func<T, bool> condition)
    {
        return task.Then(t =>
        {
            if (condition(t))
            {
                action(t);
            }
        });
    }

    public static Task<T> ThenIf<T>(this Task<T> task, Action<T> action, bool condition)
    {
        return task.Then(t =>
        {
            if (condition)
            {
                action(t);
            }
        });
    }

    public static Task<T> ThenIf<T>(this Task<T> task, Func<T, Task> action, Func<T, bool> condition)
    {
        return task.Then(t => condition(t) ? action(t) : Task.CompletedTask);
    }

    public static Task<T> ThenIf<T>(this Task<T> task, Func<T, Task> action, bool condition)
    {
        return task.Then(t => condition ? action(t) : Task.CompletedTask);
    }


    public static Task When(this Task task, bool condition, Func<Task> action)
    {
        return task.Then(() => condition ? action() : task);
    }

    public static Task When(this Task task, bool condition, Action action)
    {
        return task.Then(() =>
        {
            if (condition)
                action();
        });
    }

    public static Task<T> When<T>(this Task<T> task, Func<T, bool> condition, Func<T, Task> action)
    {
        return task.Then(t => condition(t) ? action(t) : Task.CompletedTask);
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
}