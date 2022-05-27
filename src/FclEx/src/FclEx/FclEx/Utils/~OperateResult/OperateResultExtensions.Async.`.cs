using System;
using System.Threading.Tasks;
using FclEx.Extensions;

namespace FclEx.Utils;

partial class OperateResultExtensions
{
    public static Task<OperateResult<T>> OkResult<T>(this Task<OperateResult<T>> @this, Action<OperateResult<T>> action)
    {
        return @this.On(m => m.Success, action);
    }

    public static Task<OperateResult<T>> OkResult<T>(this Task<OperateResult<T>> @this, Func<OperateResult<T>, Task> action)
    {
        return @this.On(m => m.Success, action);
    }

    public static Task<OperateResult<T>> ErrorResult<T>(this Task<OperateResult<T>> @this, Action<OperateResult<T>> action)
    {
        return @this.On(r => r.Error, action);
    }

    public static Task<OperateResult<T>> ErrorResult<T>(this Task<OperateResult<T>> @this, Func<OperateResult<T>, Task> action)
    {
        return @this.On(r => r.Error, action);
    }

    public static Task<OperateResult<T>> CancelResult<T>(this Task<OperateResult<T>> @this, Action<OperateResult<T>> action)
    {
        return @this.On(r => r.IsCancelErr(), action);
    }

    public static Task<OperateResult<T>> CancelResult<T>(this Task<OperateResult<T>> @this, Func<OperateResult<T>, Task> action)
    {
        return @this.On(r => r.IsCancelErr(), action);
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

    public static Task<OperateResult<T>> Error<T>(this Task<OperateResult<T>> @this, Action<Exception, TimeSpan> action)
    {
        return @this.ErrorResult(t => action(t.Exception!, t.Elapsed));
    }

    public static Task<OperateResult<T>> Error<T>(this Task<OperateResult<T>> @this, Action<Exception> action)
    {
        return @this.Error((e, t) => action(e));
    }

    public static Task<OperateResult<T>> Error<T>(this Task<OperateResult<T>> @this, Func<Exception, TimeSpan, Task> action)
    {
        return @this.ErrorResult(t => action(t.Exception!, t.Elapsed));
    }

    public static Task<OperateResult<T>> Error<T>(this Task<OperateResult<T>> @this, Func<Exception, Task> action)
    {
        return @this.Error((e, t) => action(e));
    }

    public static Task<OperateResult<T>> ThrowIfError<T>(this Task<OperateResult<T>> @this)
    {
        return @this.Error(e => e.ReThrow());
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