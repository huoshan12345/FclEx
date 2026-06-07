namespace FclEx.Utils;

public static partial class Operation
{
    public static OperationResult<T> NotImplemented<T>() => Error<T>(new NotImplementedException().SetStackTrace());

    public static OperationResult<T> Cancel<T>(Exception exception, TimeSpan elapsed = default)
    {
        Check.NotNull(exception);
        return Error<T>(exception is OperationCanceledException ? exception : new OperationCanceledException(exception.Message, exception).SetStackTrace(), elapsed);
    }

    public static OperationResult<T> Cancel<T>(TimeSpan elapsed = default) => Error<T>(new OperationCanceledException().SetStackTrace(), elapsed);

    public static OperationResult<T> Success<T>(T value, TimeSpan elapsed = default) => OperationResult<T>.FromSuccess(value, elapsed);

    public static OperationResult<T> Error<T>(Exception exception, TimeSpan elapsed = default)
    {
        Check.NotNull(exception);
        return new(exception, elapsed);
    }

    public static OperationResult<T> Error<T>(string error, TimeSpan elapsed = default) => Error<T>(new SimpleException(Check.NotNull(error)), elapsed);

    public static OperationResult<T> ObjectError<T>(T value, string error, TimeSpan elapsed = default) where T : notnull
    {
        Check.NotNull(value);
        Check.NotNull(error);

        return new(ObjectException.Create(value, error).SetStackTrace(), elapsed);
    }

    public static OperationResult<T> ObjectError<T>(T value, Exception exception, TimeSpan elapsed = default) where T : notnull
    {
        Check.NotNull(value);
        Check.NotNull(exception);

        return new(ObjectException.Create(value, exception.Message, exception).SetStackTrace(), elapsed);
    }

    public static OperationResult<TResult> ObjectError<T, TResult>(T value, Exception exception, TimeSpan elapsed = default) where T : notnull
    {
        Check.NotNull(value);
        Check.NotNull(exception);

        var objectException = ObjectException.Create(value, exception.Message, exception).SetStackTrace();
        return new(objectException, elapsed);
    }
}
