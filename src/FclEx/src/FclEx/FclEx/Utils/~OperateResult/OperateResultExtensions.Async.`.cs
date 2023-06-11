namespace FclEx.Utils;

partial class OperateResultExtensions
{
    public static Task<OperateResult<T>> OkResult<T>(this Task<OperateResult<T>> task, Action<OperateResult<T>> action)
    {
        return task.On(m => m.Success, action);
    }

    public static Task<OperateResult<T>> OkResult<T>(this Task<OperateResult<T>> task, Func<OperateResult<T>, Task> action)
    {
        return task.On(m => m.Success, action);
    }

    public static Task<OperateResult<T>> ErrorResult<T>(this Task<OperateResult<T>> task, Action<OperateResult<T>> action)
    {
        return task.On(r => r.Error, action);
    }

    public static Task<OperateResult<T>> ErrorResult<T>(this Task<OperateResult<T>> task, Func<OperateResult<T>, Task> action)
    {
        return task.On(r => r.Error, action);
    }

    public static Task<OperateResult<T>> CancelResult<T>(this Task<OperateResult<T>> task, Action<OperateResult<T>> action)
    {
        return task.On(r => r.IsCanceled(), action);
    }

    public static Task<OperateResult<T>> CancelResult<T>(this Task<OperateResult<T>> task, Func<OperateResult<T>, Task> action)
    {
        return task.On(r => r.IsCanceled(), action);
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
        return task.Error(e => e.ReThrow());
    }

    public static Task<OperateResult> Untype<T>(this Task<OperateResult<T>> task)
    {
        return task.ContinueWith(t => t.Result.Untype());
    }

    public static Task<OperateResult<TNext>> Next<T, TNext>(this Task<OperateResult<T>> task, Func<T, Task<OperateResult<TNext>>> func)
    {
        return task.ContinueWith<OperateResult<T>, OperateResult<TNext>>(t => t.Result.Success 
            ? func(t.Result.Value) 
            : t.Result.ToExplicit<TNext>().ToTask());
    }

    public static Task<OperateResult<TNext>> NextResult<T, TNext>(this Task<OperateResult<T>> task, Func<OperateResult<T>, Task<OperateResult<TNext>>> func)
    {
        return task.ContinueWith<OperateResult<T>, OperateResult<TNext>>(t => func(t.Result));
    }
}