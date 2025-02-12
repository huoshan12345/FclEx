namespace FclEx.Utils;

public partial class Operation
{
    public static OperationResult<T> NotImplemented<T>() => Error<T>(OperationResultCodes.NotImplemented, "the operation was not implemented", default);

    public static OperationResult<T> Cancel<T>(Exception ex, TimeSpan elapsed = default) => new(OperationResultCodes.Canceled, ex, elapsed);

    public static OperationResult<T> Cancel<T>(TimeSpan elapsed = default) => Error<T>(OperationResultCodes.Canceled, "the operation was canceled", elapsed);

    public static OperationResult<T> Success<T>(T item, TimeSpan elapsed = default) => new(item!, elapsed);

    public static OperationResult<T> Error<T>(int code, string? error, TimeSpan elapsed = default) => new(code, new SimpleException(error), elapsed);

    public static OperationResult<T> Error<T>(string? error, TimeSpan elapsed = default) => Error<T>(OperationResultCodes.StringError, error, elapsed);

    public static OperationResult<T> Error<T>(Exception ex, TimeSpan elapsed = default)
    {
        return new OperationResult<T>(IsCanceled(ex) ? OperationResultCodes.Canceled : OperationResultCodes.Exception, ex, elapsed);
    }

    public static OperationResult<T> ObjectError<T>(T obj, string error, TimeSpan elapsed = default) where T : notnull
    {
        return new(OperationResultCodes.StringError, ObjectException.Create(obj, error), elapsed);
    }

    public static OperationResult<T> ObjectError<T>(T obj, Exception ex, TimeSpan elapsed = default) where T : notnull
    {
        var code = IsCanceled(ex) ? OperationResultCodes.Canceled : OperationResultCodes.Exception;
        return new(code, ObjectException.Create(obj, ex.Message, ex), elapsed);
    }

    public static OperationResult<TResult> ObjectError<T, TResult>(T obj, Exception ex, TimeSpan elapsed = default) where T : notnull
    {
        var code = IsCanceled(ex)
            ? OperationResultCodes.Canceled
            : OperationResultCodes.Exception;
        var objEx = ObjectException.Create(obj, ex.Message, ex);
        return new(code, objEx, elapsed);
    }
}