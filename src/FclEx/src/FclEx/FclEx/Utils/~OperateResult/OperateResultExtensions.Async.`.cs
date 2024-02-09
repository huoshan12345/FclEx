namespace FclEx.Utils;

partial class OperateResultExtensions
{
    public static Task<OperateResult<T>> OkResult<T>(this Task<OperateResult<T>> task, Action<OperateResult<T>> action)
    {
        return task.Do(m => m.Success, action);
    }

    public static Task<OperateResult<T>> OkResult<T>(this Task<OperateResult<T>> task, Func<OperateResult<T>, Task> action)
    {
        return task.Do(m => m.Success, action);
    }

    public static Task<OperateResult<T>> ErrorResult<T>(this Task<OperateResult<T>> task, Action<OperateResult<T>> action)
    {
        return task.Do(r => r.Error, action);
    }

    public static Task<OperateResult<T>> ErrorResult<T>(this Task<OperateResult<T>> task, Func<OperateResult<T>, Task> action)
    {
        return task.Do(r => r.Error, action);
    }

    public static Task<OperateResult<T>> CancelResult<T>(this Task<OperateResult<T>> task, Action<OperateResult<T>> action)
    {
        return task.Do(r => r.IsCanceled(), action);
    }

    public static Task<OperateResult<T>> CancelResult<T>(this Task<OperateResult<T>> task, Func<OperateResult<T>, Task> action)
    {
        return task.Do(r => r.IsCanceled(), action);
    }

    public static Task<OperateResult<T>> Ok<T>(this Task<OperateResult<T>> task, Action<T, TimeSpan> action)
    {
        return task.OkResult(t => action(t.Value!, t.Elapsed));
    }

    public static Task<OperateResult<T>> Ok<T>(this Task<OperateResult<T>> task, Action<T> action)
    {
        return task.Ok((r, t) => action(r));
    }

    public static Task<OperateResult<T>> Ok<T>(this Task<OperateResult<T>> task, Func<T, TimeSpan, Task> action)
    {
        return task.OkResult(t => action(t.Value!, t.Elapsed));
    }

    public static Task<OperateResult<T>> Ok<T>(this Task<OperateResult<T>> task, Func<T, Task> action)
    {
        return task.Ok((r, t) => action(r));
    }

    public static Task<OperateResult<T>> Error<T>(this Task<OperateResult<T>> task, Action<Exception, TimeSpan> action)
    {
        return task.ErrorResult(t => action(t.Exception!, t.Elapsed));
    }

    public static Task<OperateResult<T>> Error<T>(this Task<OperateResult<T>> task, Action<Exception> action)
    {
        return task.Error((e, t) => action(e));
    }

    public static Task<OperateResult<T>> Error<T>(this Task<OperateResult<T>> task, Func<Exception, TimeSpan, Task> action)
    {
        return task.ErrorResult(t => action(t.Exception!, t.Elapsed));
    }

    public static Task<OperateResult<T>> Error<T>(this Task<OperateResult<T>> task, Func<Exception, Task> action)
    {
        return task.Error((e, t) => action(e));
    }

    public static Task<OperateResult<T>> ThrowIfError<T>(this Task<OperateResult<T>> task)
    {
        return task.NextResult(m => m.ThrowIfError());
    }

    public static Task<OperateResult> Untype<T>(this Task<OperateResult<T>> task)
    {
        return task.ContinueWith(t => t.Result.Untype());
    }

    public static Task<OperateResult<TNext>> Next<T, TNext>(this Task<OperateResult<T>> task, Func<T, Task<OperateResult<TNext>>> next)
    {
        var watch = ValueStopwatch.StartNew();
        return task.ContinueWith(async m =>
        {
            var elapsed = watch.GetElapsedTime();

            if (task.Exception is { } ex)
                return Operate.CreateError<TNext>(ex, elapsed);

            if (task.IsCanceled)
                return Operate.CreateCancel<TNext>(elapsed);

            return m.Result.Success
                ? await next(m.Result.Value)
                : m.Result.ToExplicit<TNext>();
        }).Unwrap();
    }

    public static Task<OperateResult<TNext>> Next<T, TNext>(this Task<OperateResult<T>> task, Func<T, OperateResult<TNext>> next)
    {
        return task.Next(m => next(m).ToTask());
    }

    public static Task<OperateResult<TNext>> Next<T, TNext>(this Task<OperateResult<T>> task, Func<T, TNext> next)
    {
        return task.Next(m => Operate.CreateSuccess(next(m)));
    }

    public static Task<OperateResult<TNext>> NextResult<T, TNext>(this Task<OperateResult<T>> task, Func<OperateResult<T>, Task<OperateResult<TNext>>> next)
    {
        var watch = ValueStopwatch.StartNew();
        return task.ContinueWith(async m =>
        {
            var elapsed = watch.GetElapsedTime();

            if (task.Exception is { } ex)
                return Operate.CreateError<TNext>(ex, elapsed);

            if (task.IsCanceled)
                return Operate.CreateCancel<TNext>(elapsed);

            return await next(m.Result);
        }).Unwrap();
    }

    public static Task<OperateResult<TNext>> NextResult<T, TNext>(this Task<OperateResult<T>> task, Func<OperateResult<T>, OperateResult<TNext>> next)
    {
        return task.NextResult(m => next(m).ToTask());
    }

    public static Task<OperateResult<TNext>> NextResult<T, TNext>(this Task<OperateResult<T>> task, Func<OperateResult<T>, TNext> next)
    {
        return task.NextResult(m => Operate.CreateSuccess(next(m)));
    }

    public static Task<T> GetRequiredValue<T>(this Task<OperateResult<T>> task)
    {
        return task.ContinueWith(m => m.Result.GetRequiredValue());
    }

    public static Task<Transput<TInput, OperateResult<TOutput>>> ToTransput<TInput, TOutput>(this Task<OperateResult<TOutput>> task, TInput input)
    {
        return task.Continue(m => Transput.Create(input, m));
    }
}