namespace FclEx.Utils;

partial class OperationResultExtensions
{
    public static OperationResult<T> When<T>(this OperationResult<T> result, Func<T, bool> condition, Action<OperationResult<T>> action)
    {
        if (result.IsSuccess && condition(result.Value))
            action(result);
        return result;
    }

    public static OperationResult<T> WhenResult<T>(this OperationResult<T> result, Func<OperationResult<T>, bool> condition, Action<OperationResult<T>> action)
    {
        if (condition(result))
            action(result);
        return result;
    }

    public static OperationResult<T> OnSucceeded<T>(this OperationResult<T> result, Action<OperationResult<T>> action)
    {
        return result.WhenResult(m => m.IsSuccess, action);
    }

    public static OperationResult<T> OnFailed<T>(this OperationResult<T> result, Action<OperationResult<T>> action)
    {
        return result.WhenResult(m => m.IsError, action);
    }

    public static OperationResult<T> OnFaulted<T>(this OperationResult<T> result, Action<OperationResult<T>> action)
    {
        return result.WhenResult(m => m.IsFaulted(), action);
    }

    public static OperationResult<T> OnCanceled<T>(this OperationResult<T> result, Action<OperationResult<T>> action)
    {
        return result.WhenResult(m => m.IsCanceled(), action);
    }

    public static OperationResult<T> OnValue<T>(this OperationResult<T> result, Action<T> action)
    {
        return result.OnSucceeded(m => action(m.Value!));
    }

    public static OperationResult<T> OnValue<T>(this OperationResult<T> result, Action<T, TimeSpan> action)
    {
        return result.OnSucceeded(m => action(m.Value!, m.Elapsed));
    }

    public static OperationResult<T> OnException<T>(this OperationResult<T> result, Action<Exception> action)
    {
        return result.OnFailed(t => action(t.Exception!));
    }

    public static OperationResult<T> OnException<T>(this OperationResult<T> result, Action<Exception, TimeSpan> action)
    {
        return result.OnFailed(t => action(t.Exception!, t.Elapsed));
    }

    public static OperationResult WithoutValue<T>(this OperationResult<T> result) => result;

    public static OperationResult<T> Elapsed<T>(this OperationResult<T> result, TimeSpan span)
    {
        return result.IsSuccess
            ? (result.Value, span)
            : (result.Exception!, span);
    }

    public static OperationResult<T> ThrowIfError<T>(this OperationResult<T> result)
    {
        if (result.IsError)
            result.Exception.ReThrow();
        return result;
    }

    public static OperationResult<T> Unwrap<T>(this OperationResult<OperationResult<T>> result)
    {
        return result.IsSuccess
            ? result.Value
            : (result.Exception, result.Elapsed);
    }

    public static OperationResult<TResult> MapValue<T, TResult>(this OperationResult<T> result, Func<T, TResult> func)
    {
        return result.IsSuccess
            ? (func(result.Value)!, result.Elapsed)
            : (result.Exception, result.Elapsed);
    }

    public static OperationResult<TResult> Then<T, TResult>(this OperationResult<T> result, Func<T, OperationResult<TResult>> func)
    {
        return result.IsSuccess
            ? func(result.Value)
            : (result.Exception, result.Elapsed);
    }

    public static OperationResult<TResult> ThenResult<T, TResult>(this OperationResult<T> result, Func<OperationResult<T>, OperationResult<TResult>> func)
    {
        return func(result);
    }

    public static OperationResult<TResult> ThenResult<T, TResult>(this OperationResult<T> result, Func<OperationResult<T>, TResult> func)
    {
        return func(result);
    }

    public static T Unwrap<T>(this OperationResult<T> result, T defaultValue)
    {
        return result.IsSuccess ? result.Value : defaultValue;
    }

    public static T Unwrap<T>(this OperationResult<T> result)
    {
        if (result.IsSuccess == false)
            result.Exception.ReThrow();

        return result.Value;
    }

    public static T FromObjectError<T>(this IOperationResult result) where T : notnull
    {
        if (result.IsObjectError<T>(static (_, _) => true, out var value))
            return value;

        throw new InvalidOperationException($"The result is not an object error of type '{typeof(T).LongName()}'");
    }

    public static T FromObjectError<T>(this IOperationResult<T> result) where T : notnull
    {
        return result.CastTo<IOperationResult>().FromObjectError<T>();
    }

    public static bool TryGetValue<T>(this OperationResult<T> result, [NotNullWhen(true)] out T? value)
    {
        if (result.IsSuccess)
        {
            value = result.Value;
            return true;
        }

        value = default;
        return false;
    }

    public static bool IsSuccess<T>(this OperationResult<T> result, Func<T, bool> condition)
    {
        return result.IsSuccess && condition(result.Value);
    }
}