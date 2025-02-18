namespace FclEx.Utils;

partial class OperationResultExtensions
{
    public static OperationResult WithoutValue<T>(this OperationResult<T> result) => result;

    public static OperationResult<T> Elapsed<T>(this OperationResult<T> result, TimeSpan span)
    {
        return result.Success
            ? (result.Value, span)
            : (result.Exception!, span);
    }

    public static OperationResult<T> ThrowIfError<T>(this OperationResult<T> result)
    {
        if (result.Error)
            result.Exception.ReThrow();
        return result;
    }

    public static OperationResult<T> Unwrap<T>(this OperationResult<OperationResult<T>> result)
    {
        return result.Success
            ? result.Value
            : (result.Exception, result.Elapsed);
    }

    public static OperationResult<TResult> Map<T, TResult>(this OperationResult<T> result, Func<T, TResult> func)
    {
        return result.Success
            ? (func(result.Value)!, result.Elapsed)
            : (result.Exception, result.Elapsed);
    }

    public static OperationResult<TResult> Bind<T, TResult>(this OperationResult<T> result, Func<T, OperationResult<TResult>> func)
    {
        return result.Success
            ? func(result.Value)
            : (result.Exception, result.Elapsed);
    }

    public static OperationResult<TResult> Apply<T, TResult>(this OperationResult<T> result, Func<OperationResult<T>, OperationResult<TResult>> func)
    {
        return func(result);
    }

    public static OperationResult<TResult> Apply<T, TResult>(this OperationResult<T> result, Func<OperationResult<T>, TResult> func)
    {
        return func(result);
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

    public static T Unwrap<T>(this OperationResult<T> result, T defaultValue)
    {
        return result.Success ? result.Value : defaultValue;
    }

    public static T Unwrap<T>(this OperationResult<T> result)
    {
        if (result.Success == false)
            result.Exception.ReThrow();

        return result.Value;
    }
}