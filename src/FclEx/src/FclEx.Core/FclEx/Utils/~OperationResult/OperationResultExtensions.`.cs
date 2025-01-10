namespace FclEx.Utils;

partial class OperationResultExtensions
{
    public static OperationResult Untype<T>(this OperationResult<T> result) => result;

    public static OperationResult<T> Elapsed<T>(this OperationResult<T> result, TimeSpan span)
    {
        return result.Success
            ? new OperationResult<T>(result.Value, span)
            : new OperationResult<T>(result.Code, result.Exception!, span);
    }

    public static OperationResult<T> ThrowIfError<T>(this OperationResult<T> result)
    {
        if (result.Error)
            result.Exception.ReThrow();
        return result;
    }

    public static OperationResult<T> Unwrap<T>(this OperationResult<OperationResult<T>> result)
    {
        var (successful, innerResult, ex, elapsed) = result;
        return successful
            ? innerResult
            : Operation.CreateError<T>(ex!, elapsed);
    }

    public static OperationResult<TDest> Map<T, TDest>(this OperationResult<T> result, Func<T, TDest> func)
    {
        return result.Success
            ? Operation.CreateSuccess(func(result.Value!))
            : result.ToExplicit<TDest>();
    }

    public static OperationResult<TDest> Bind<T, TDest>(this OperationResult<T> result, Func<T, OperationResult<TDest>> func)
    {
        return result.Success
            ? func(result.Value!)
            : result.ToExplicit<TDest>();
    }

    public static OperationResult<T> On<T>(this OperationResult<T> result, Func<OperationResult<T>, bool> condition, Action<OperationResult<T>> action)
    {
        if (condition(result))
            action(result);
        return result;
    }

    public static OperationResult<T> OkResult<T>(this OperationResult<T> result, Action<OperationResult<T>> action)
    {
        return result.On(m => m.Success, action);
    }

    public static OperationResult<T> ErrorResult<T>(this OperationResult<T> result, Action<OperationResult<T>> action)
    {
        return result.On(m => m.Error, action);
    }

    public static OperationResult<T> Ok<T>(this OperationResult<T> result, Action<T, TimeSpan> action)
    {
        return result.OkResult(r => action(r.Value!, r.Elapsed));
    }

    public static OperationResult<T> Ok<T>(this OperationResult<T> result, Action<T> action)
    {
        return result.OkResult(r => action(r.Value!));
    }

    public static OperationResult<T> Error<T>(this OperationResult<T> result, Action<Exception, TimeSpan> action)
    {
        return result.ErrorResult(r => action(r.Exception!, r.Elapsed));
    }

    public static OperationResult<T> Error<T>(this OperationResult<T> result, Action<Exception> action)
    {
        return result.ErrorResult(r => action(r.Exception!));
    }

    public static T GetRequiredValue<T>(this OperationResult<T> result)
    {
        if (result.Success == false)
            result.Exception.ReThrow();

        return result.Value;
    }
}