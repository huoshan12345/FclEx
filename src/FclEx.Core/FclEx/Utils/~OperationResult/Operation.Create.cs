namespace FclEx.Utils;

public partial class Operation
{
    private static bool IsCanceled(Exception ex)
    {
        var t = ex.GetType();
        var isCancel = t == typeof(TaskCanceledException) || t == typeof(OperationCanceledException);
        return isCancel;
    }

    public static OperationResult Cancel(Exception ex, TimeSpan elapsed = default) => new(OperationResultCodes.Canceled, ex, elapsed);

    public static OperationResult Cancel(TimeSpan elapsed = default) => Error(OperationResultCodes.Canceled, "the operation was canceled", elapsed);

    public static OperationResult Success(TimeSpan elapsed = default) => new(default, elapsed);

    public static OperationResult Error(int code, Exception ex, TimeSpan elapsed = default) => new(code, ex, elapsed);

    public static OperationResult Error(int code, string? error, TimeSpan elapsed = default) => Error(code, new SimpleException(error), elapsed);

    public static OperationResult Error(string? error, TimeSpan elapsed = default) => Error(OperationResultCodes.StringError, error, elapsed);

    public static OperationResult Error(Exception ex, TimeSpan elapsed = default)
    {
        return new(IsCanceled(ex) ? OperationResultCodes.Canceled : OperationResultCodes.Exception, ex, elapsed);
    }

    public static OperationResult NotImplemented() => Error(OperationResultCodes.NotImplemented, "the operation was not implemented", default);
}