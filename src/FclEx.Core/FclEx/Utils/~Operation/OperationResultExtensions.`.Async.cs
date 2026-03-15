using TaskExtensions = FclEx.Extensions.TaskExtensions;

namespace FclEx.Utils;

partial class OperationResultExtensions
{
    public static Task<OperationResult<T>> When<T>(this Task<OperationResult<T>> result, Func<T, bool> condition, Func<T, Task> action)
    {
        Check.NotNull(condition);
        Check.NotNull(action);

        return result.WhenResult(r => r.IsSuccess && condition(r.Value), r => action(r.Value!));
    }

    public static Task<OperationResult<T>> When<T>(this Task<OperationResult<T>> result, Func<T, bool> condition, Action<T> action)
    {
        Check.NotNull(action);

        return result.When(condition, v =>
        {
            action(v);
            return Task.CompletedTask;
        });
    }

    public static Task<OperationResult<T>> WhenResult<T>(this Task<OperationResult<T>> result, Func<OperationResult<T>, bool> condition, Func<OperationResult<T>, Task> action)
    {
        Check.NotNull(condition);
        Check.NotNull(action);

        return result.Then(r => condition(r)
            ? r
            : action(r).Then(() => r));
    }

    public static Task<OperationResult<T>> WhenResult<T>(this Task<OperationResult<T>> result, Func<OperationResult<T>, bool> condition, Action<OperationResult<T>> action)
    {
        Check.NotNull(action);

        return result.WhenResult(condition, r =>
        {
            action(r);
            return Task.CompletedTask;
        });
    }

    public static Task<OperationResult<T>> OnResult<T>(this Task<OperationResult<T>> task, Action<OperationResult<T>> action)
    {
        return task.Then(action);
    }

    public static Task<OperationResult<T>> OnResult<T>(this Task<OperationResult<T>> task, Func<OperationResult<T>, Task> action)
    {
        return task.Then(action);
    }

    public static Task<OperationResult<T>> OnSucceeded<T>(this Task<OperationResult<T>> task, Action<OperationResult<T>> action)
    {
        return task.ThenIf(action, m => m.IsSuccess);
    }

    public static Task<OperationResult<T>> OnSucceeded<T>(this Task<OperationResult<T>> task, Func<OperationResult<T>, Task> action)
    {
        return task.ThenIf(action, m => m.IsSuccess);
    }

    public static Task<OperationResult<T>> OnFailed<T>(this Task<OperationResult<T>> task, Action<OperationResult<T>> action)
    {
        return task.ThenIf(action, m => m.IsError);
    }

    public static Task<OperationResult<T>> OnFailed<T>(this Task<OperationResult<T>> task, Func<OperationResult<T>, Task> action)
    {
        return task.ThenIf(action, m => m.IsError);
    }

    public static Task<OperationResult<T>> OnFaulted<T>(this Task<OperationResult<T>> task, Action<OperationResult<T>> action)
    {
        return task.ThenIf(action, m => m.IsFaulted());
    }

    public static Task<OperationResult<T>> OnFaulted<T>(this Task<OperationResult<T>> task, Func<OperationResult<T>, Task> action)
    {
        return task.ThenIf(action, m => m.IsError);
    }

    public static Task<OperationResult<T>> OnCanceled<T>(this Task<OperationResult<T>> task, Action<OperationResult<T>> action)
    {
        return task.ThenIf(action, m => m.IsCanceled());
    }

    public static Task<OperationResult<T>> OnCanceled<T>(this Task<OperationResult<T>> task, Func<OperationResult<T>, Task> action)
    {
        return task.ThenIf(action, m => m.IsCanceled());
    }

    public static Task<OperationResult<T>> OnValue<T>(this Task<OperationResult<T>> task, Action<T> action)
    {
        return task.OnSucceeded(m => action(m.Value!));
    }

    public static Task<OperationResult<T>> OnValue<T>(this Task<OperationResult<T>> task, Func<T, Task> action)
    {
        return task.OnSucceeded(m => action(m.Value!));
    }

    public static Task<OperationResult<T>> OnValue<T>(this Task<OperationResult<T>> task, Action<T, TimeSpan> action)
    {
        return task.OnSucceeded(m => action(m.Value!, m.Elapsed));
    }

    public static Task<OperationResult<T>> OnValue<T>(this Task<OperationResult<T>> task, Func<T, TimeSpan, Task> action)
    {
        return task.OnSucceeded(m => action(m.Value!, m.Elapsed));
    }

    public static Task<OperationResult<T>> OnException<T>(this Task<OperationResult<T>> task, Action<Exception> action)
    {
        return task.OnFailed(t => action(t.Exception!));
    }

    public static Task<OperationResult<T>> OnException<T>(this Task<OperationResult<T>> task, Action<Exception, TimeSpan> action)
    {
        return task.OnFailed(t => action(t.Exception!, t.Elapsed));
    }

    public static Task<OperationResult<T>> OnException<T>(this Task<OperationResult<T>> task, Func<Exception, TimeSpan, Task> action)
    {
        return task.OnFailed(t => action(t.Exception!, t.Elapsed));
    }

    public static Task<OperationResult<T>> OnException<T>(this Task<OperationResult<T>> task, Func<Exception, Task> action)
    {
        return task.OnFailed(t => action(t.Exception!));
    }

    public static Task<OperationResult<T>> ThrowIfError<T>(this Task<OperationResult<T>> task)
    {
        return task.ContinueWith(m =>
        {
            if (task.Exception is { } ex)
                ex.GetBaseException().ReThrow();

            if (task.IsCanceled)
                throw new TaskCanceledException(m);

            return m.Result.ThrowIfError();
        });
    }

    public static Task<OperationResult> WithoutValue<T>(this Task<OperationResult<T>> task)
    {
        return task.ContinueWith(t => t.Result.WithoutValue());
    }

    public static Task<OperationResult<TNext>> Then<T, TNext>(this Task<OperationResult<T>> task, Func<T, Task<OperationResult<TNext>>> next)
    {
        var watch = ValueStopwatch.StartNew();
        return task.ContinueWith(async m =>
        {
            var elapsed = watch.GetElapsedTime();

            if (task.Exception is { } ex)
                return Operation.Error<TNext>(ex.GetBaseException(), elapsed);

            if (task.IsCanceled)
                return Operation.Cancel<TNext>(elapsed);

            return m.Result.IsSuccess
                ? await next(m.Result.Value)
                : m.Result.Cast<TNext>();
        }).Unwrap();
    }

    public static Task<OperationResult<TNext>> Then<T, TNext>(this Task<OperationResult<T>> task, Func<T, OperationResult<TNext>> next)
    {
        return task.Then(m => Task.FromResult(next(m)));
    }

    public static Task<OperationResult<TNext>> Then<T, TNext>(this Task<OperationResult<T>> task, Func<T, TNext> next)
    {
        return task.Then(m => Operation.Success(next(m)));
    }

    /// <summary>
    /// Alias for Then with <see cref="Func{T, TNext}"/> to avoid ambiguous.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TNext"></typeparam>
    /// <param name="task"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public static Task<OperationResult<TNext>> MapValue<T, TNext>(this Task<OperationResult<T>> task, Func<T, TNext> next)
    {
        return task.Then(next);
    }

    public static Task<OperationResult<TNext>> ThenResult<T, TNext>(this Task<OperationResult<T>> task, Func<OperationResult<T>, Task<OperationResult<TNext>>> next)
    {
        var watch = ValueStopwatch.StartNew();
        return task.ContinueWith(async m =>
        {
            var elapsed = watch.GetElapsedTime();

            if (task.Exception is { } ex)
                return Operation.Error<TNext>(ex.GetBaseException(), elapsed);

            if (task.IsCanceled)
                return Operation.Cancel<TNext>(elapsed);

            return await next(m.Result);
        }).Unwrap();
    }

    public static Task<OperationResult<TNext>> ThenResult<T, TNext>(this Task<OperationResult<T>> task, Func<OperationResult<T>, OperationResult<TNext>> next)
    {
        return task.ThenResult(m => Task.FromResult(next(m)));
    }

    public static Task<OperationResult<TNext>> ThenResult<T, TNext>(this Task<OperationResult<T>> task, Func<OperationResult<T>, TNext> next)
    {
        return task.ThenResult(m => Operation.Success(next(m)));
    }

    public static Task<T> Unwrap<T>(this Task<OperationResult<T>> task)
    {
        return task.ContinueWith(m => m.Result.Unwrap());
    }

    public static Task<T> Unwrap<T>(this Task<OperationResult<T>> task, T defaultValue)
    {
        return task.ContinueWith(m => m.Result.Unwrap(defaultValue));
    }

    public static Task<IOPair<TInput, OperationResult<TOutput>>> ToIOPair<TInput, TOutput>(this Task<OperationResult<TOutput>> task, TInput input)
    {
        return TaskExtensions.Then(task, m => IOPair.Create(input, m));
    }

    public static Task<OperationResult<TNext>> ThenIf<T, TNext>(this Task<OperationResult<T>> task, Func<T, bool> condition,
        Func<T, Task<OperationResult<TNext>>> @true, Func<T, Task<OperationResult<TNext>>> @false)
    {
        Check.NotNull(condition);
        Check.NotNull(@true);
        Check.NotNull(@false);

        return task.Then(t => condition(t) ? @true(t) : @false(t));
    }

    public static Task<OperationResult<T>> ThenIf<T>(this Task<OperationResult<T>> task, Func<T, bool> condition, Func<T, Task<OperationResult<T>>> next)
    {
        return task.ThenIf(condition, next, m => Operation.Success(m));
    }

    public static Task<OperationResult> ThenIf<T>(this Task<OperationResult<T>> task, Func<T, bool> condition, Func<T, Task<OperationResult>> next)
    {
        return task.ThenIf(condition, next, _ => Operation.Success(Unit.Default));
    }
}