namespace FclEx.Utils;

partial class OperationResultExtensions
{
    public static OperationResult AsEmpty<T>(this OperationResult<T> result) => result;

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
            : Operation.Error<T>(ex!, elapsed);
    }

    public static OperationResult<TResult> Map<T, TResult>(this OperationResult<T> result, Func<T, TResult> func)
    {
        return result.Success
            ? new OperationResult<TResult>(func(result.Value)!, result.Elapsed)
            : new OperationResult<TResult>(result.Code, result.Exception, result.Elapsed);
    }

    public static OperationResult<TResult> Bind<T, TResult>(this OperationResult<T> result, Func<T, OperationResult<TResult>> func)
    {
        return result.Success
            ? func(result.Value)
            : new OperationResult<TResult>(result.Code, result.Exception, result.Elapsed);
    }

    public static OperationResult<T> On<T>(this OperationResult<T> result, Func<OperationResult<T>, bool> condition, Action<OperationResult<T>> action)
    {
        if (condition(result))
            action(result);
        return result;
    }

    public static OperationResult<T> SuccessResult<T>(this OperationResult<T> result, Action<OperationResult<T>> action)
    {
        return result.On(m => m.Success, action);
    }

    public static OperationResult<T> ErrorResult<T>(this OperationResult<T> result, Action<OperationResult<T>> action)
    {
        return result.On(m => m.Error, action);
    }

    public static OperationResult<T> Success<T>(this OperationResult<T> result, Action<T, TimeSpan> action)
    {
        return result.SuccessResult(r => action(r.Value!, r.Elapsed));
    }

    public static OperationResult<T> Success<T>(this OperationResult<T> result, Action<T> action)
    {
        return result.SuccessResult(r => action(r.Value!));
    }

    public static OperationResult<T> Error<T>(this OperationResult<T> result, Action<Exception, TimeSpan> action)
    {
        return result.ErrorResult(r => action(r.Exception!, r.Elapsed));
    }

    public static OperationResult<T> Error<T>(this OperationResult<T> result, Action<Exception> action)
    {
        return result.ErrorResult(r => action(r.Exception!));
    }

    public static T Unwrap<T>(this OperationResult<T> result)
    {
        if (result.Success == false)
            result.Exception.ReThrow();

        return result.Value;
    }
}