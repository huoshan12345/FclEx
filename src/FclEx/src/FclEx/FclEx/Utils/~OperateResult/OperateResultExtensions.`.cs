namespace FclEx.Utils;

partial class OperateResultExtensions
{
    public static OperateResult Untype<T>(this OperateResult<T> result) => result;

    public static OperateResult<T> Elapsed<T>(this OperateResult<T> result, TimeSpan span)
    {
        return result.Success
            ? new OperateResult<T>(result.Value, span)
            : new OperateResult<T>(result.Code, result.Exception!, span);
    }

    public static OperateResult<T> ThrowIfError<T>(this OperateResult<T> result)
    {
        if (result.Error)
            result.Exception.ReThrow();
        return result;
    }

    public static OperateResult<T> Unwrap<T>(this OperateResult<OperateResult<T>> result)
    {
        var (successful, innerResult, ex, elapsed) = result;
        return successful
            ? innerResult
            : Operate.CreateError<T>(ex!, elapsed);
    }

    public static OperateResult<TDest> Map<T, TDest>(this OperateResult<T> result, Func<T, TDest> func)
    {
        return result.Success
            ? Operate.CreateSuccess(func(result.Value!))
            : result.ToExplicit<TDest>();
    }

    public static OperateResult<TDest> Bind<T, TDest>(this OperateResult<T> result, Func<T, OperateResult<TDest>> func)
    {
        return result.Success
            ? func(result.Value!)
            : result.ToExplicit<TDest>();
    }

    public static OperateResult<T> On<T>(this OperateResult<T> result, Func<OperateResult<T>, bool> condition, Action<OperateResult<T>> action)
    {
        if (condition(result))
            action(result);
        return result;
    }

    public static OperateResult<T> OkResult<T>(this OperateResult<T> result, Action<OperateResult<T>> action)
    {
        return result.On(m => m.Success, action);
    }

    public static OperateResult<T> ErrorResult<T>(this OperateResult<T> result, Action<OperateResult<T>> action)
    {
        return result.On(m => m.Error, action);
    }

    public static OperateResult<T> Ok<T>(this OperateResult<T> result, Action<T, TimeSpan> action)
    {
        return result.OkResult(r => action(r.Value!, r.Elapsed));
    }

    public static OperateResult<T> Ok<T>(this OperateResult<T> result, Action<T> action)
    {
        return result.OkResult(r => action(r.Value!));
    }

    public static OperateResult<T> Error<T>(this OperateResult<T> result, Action<Exception, TimeSpan> action)
    {
        return result.ErrorResult(r => action(r.Exception!, r.Elapsed));
    }

    public static OperateResult<T> Error<T>(this OperateResult<T> result, Action<Exception> action)
    {
        return result.ErrorResult(r => action(r.Exception!));
    }
}