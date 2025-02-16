namespace FclEx.Utils;

partial class OperationResultExtensions
{
    public static Task<OperationResult<T>> SuccessResult<T>(this Task<OperationResult<T>> task, Action<OperationResult<T>> action)
    {
        return task.When(m => m.Success, action);
    }

    public static Task<OperationResult<T>> SuccessResult<T>(this Task<OperationResult<T>> task, Func<OperationResult<T>, Task> action)
    {
        return task.When(m => m.Success, action);
    }

    public static Task<OperationResult<T>> ErrorResult<T>(this Task<OperationResult<T>> task, Action<OperationResult<T>> action)
    {
        return task.When(r => r.Error, action);
    }

    public static Task<OperationResult<T>> ErrorResult<T>(this Task<OperationResult<T>> task, Func<OperationResult<T>, Task> action)
    {
        return task.When(r => r.Error, action);
    }

    public static Task<OperationResult<T>> CancelResult<T>(this Task<OperationResult<T>> task, Action<OperationResult<T>> action)
    {
        return task.When(r => r.IsCanceled(), action);
    }

    public static Task<OperationResult<T>> CancelResult<T>(this Task<OperationResult<T>> task, Func<OperationResult<T>, Task> action)
    {
        return task.When(r => r.IsCanceled(), action);
    }

    public static Task<OperationResult<T>> Success<T>(this Task<OperationResult<T>> task, Action<T, TimeSpan> action)
    {
        return task.SuccessResult(t => action(t.Value!, t.Elapsed));
    }

    public static Task<OperationResult<T>> Success<T>(this Task<OperationResult<T>> task, Action<T> action)
    {
        return task.Success((r, t) => action(r));
    }

    public static Task<OperationResult<T>> Success<T>(this Task<OperationResult<T>> task, Func<T, TimeSpan, Task> action)
    {
        return task.SuccessResult(t => action(t.Value!, t.Elapsed));
    }

    public static Task<OperationResult<T>> Success<T>(this Task<OperationResult<T>> task, Func<T, Task> action)
    {
        return task.Success((r, t) => action(r));
    }

    public static Task<OperationResult<T>> Error<T>(this Task<OperationResult<T>> task, Action<Exception, TimeSpan> action)
    {
        return task.ErrorResult(t => action(t.Exception!, t.Elapsed));
    }

    public static Task<OperationResult<T>> Error<T>(this Task<OperationResult<T>> task, Action<Exception> action)
    {
        return task.Error((e, t) => action(e));
    }

    public static Task<OperationResult<T>> Error<T>(this Task<OperationResult<T>> task, Func<Exception, TimeSpan, Task> action)
    {
        return task.ErrorResult(t => action(t.Exception!, t.Elapsed));
    }

    public static Task<OperationResult<T>> Error<T>(this Task<OperationResult<T>> task, Func<Exception, Task> action)
    {
        return task.Error((e, t) => action(e));
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

    public static Task<OperationResult> AsEmpty<T>(this Task<OperationResult<T>> task)
    {
        return task.ContinueWith(t => t.Result.AsEmpty());
    }

    public static Task<OperationResult<TNext>> Next<T, TNext>(this Task<OperationResult<T>> task, Func<T, Task<OperationResult<TNext>>> next)
    {
        var watch = ValueStopwatch.StartNew();
        return task.ContinueWith(async m =>
        {
            var elapsed = watch.GetElapsedTime();

            if (task.Exception is { } ex)
                return Operation.Error<TNext>(ex.GetBaseException(), elapsed);

            if (task.IsCanceled)
                return Operation.Cancel<TNext>(elapsed);

            return m.Result.Success
                ? await next(m.Result.Value)
                : m.Result.CastTo<TNext>();
        }).Unwrap();
    }

    public static Task<OperationResult<TNext>> Next<T, TNext>(this Task<OperationResult<T>> task, Func<T, OperationResult<TNext>> next)
    {
        return task.Next(m => next(m).ToTask());
    }

    public static Task<OperationResult<TNext>> Next<T, TNext>(this Task<OperationResult<T>> task, Func<T, TNext> next)
    {
        return task.Next(m => Operation.Success(next(m)));
    }

    public static Task<OperationResult<TNext>> NextResult<T, TNext>(this Task<OperationResult<T>> task, Func<OperationResult<T>, Task<OperationResult<TNext>>> next)
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

    public static Task<OperationResult<TNext>> NextResult<T, TNext>(this Task<OperationResult<T>> task, Func<OperationResult<T>, OperationResult<TNext>> next)
    {
        return task.NextResult(m => next(m).ToTask());
    }

    public static Task<OperationResult<TNext>> NextResult<T, TNext>(this Task<OperationResult<T>> task, Func<OperationResult<T>, TNext> next)
    {
        return task.NextResult(m => Operation.Success(next(m)));
    }

    public static Task<T> Unwrap<T>(this Task<OperationResult<T>> task)
    {
        return task.ContinueWith(m => m.Result.Unwrap());
    }

    public static Task<Transput<TInput, OperationResult<TOutput>>> ToTransput<TInput, TOutput>(this Task<OperationResult<TOutput>> task, TInput input)
    {
        return task.Then(m => Transput.Create(input, m));
    }
}