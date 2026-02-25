namespace FclEx.Utils;

partial class OperationResultExtensions
{
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

    public static Task<OperationResult<TNext>> ThenSucceeded<T, TNext>(this Task<OperationResult<T>> task, Func<T, Task<OperationResult<TNext>>> next)
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

    public static Task<OperationResult<TNext>> ThenSucceeded<T, TNext>(this Task<OperationResult<T>> task, Func<T, OperationResult<TNext>> next)
    {
        return task.ThenSucceeded(m => next(m).ToTask());
    }

    public static Task<OperationResult<TNext>> ThenSucceeded<T, TNext>(this Task<OperationResult<T>> task, Func<T, TNext> next)
    {
        return task.ThenSucceeded(m => Operation.Success(next(m)));
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
        return task.ThenResult(m => next(m).ToTask());
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
        return task.Then(m => IOPair.Create(input, m));
    }
}