namespace FclEx.Utils;

public partial class Operation
{
    public static OperationResult<T> NotImplemented<T>() => Error<T>(new NotImplementedException());

    public static OperationResult<T> Cancel<T>(Exception ex, TimeSpan elapsed = default) => Error<T>(ex is OperationCanceledException ? ex : new OperationCanceledException(ex.Message, ex), elapsed);

    public static OperationResult<T> Cancel<T>(TimeSpan elapsed = default) => Error<T>(new OperationCanceledException(), elapsed);

    public static OperationResult<T> Success<T>(T value, TimeSpan elapsed = default) => OperationResult<T>.FromSuccess(value, elapsed);

    public static OperationResult<T> Error<T>(Exception ex, TimeSpan elapsed = default) => new(ex, elapsed);

    public static OperationResult<T> Error<T>(string? error, TimeSpan elapsed = default) => Error<T>(new SimpleException(error), elapsed);

    public static OperationResult<T> ObjectError<T>(T obj, string error, TimeSpan elapsed = default) where T : notnull
    {
        return new(ObjectException.Create(obj, error), elapsed);
    }

    public static OperationResult<T> ObjectError<T>(T obj, Exception ex, TimeSpan elapsed = default) where T : notnull
    {
        return new(ObjectException.Create(obj, ex.Message, ex), elapsed);
    }

    public static OperationResult<TResult> ObjectError<T, TResult>(T obj, Exception ex, TimeSpan elapsed = default) where T : notnull
    {
        var objEx = ObjectException.Create(obj, ex.Message, ex);
        return new(objEx, elapsed);
    }
}