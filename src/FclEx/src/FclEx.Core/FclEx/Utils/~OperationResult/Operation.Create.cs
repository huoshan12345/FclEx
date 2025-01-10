namespace FclEx.Utils;

public partial class Operation
{
    private static bool IsCancelException(Exception ex)
    {
        var t = ex.GetType();
        var isCancel = t == typeof(TaskCanceledException) || t == typeof(OperationCanceledException);
        return isCancel;
    }

    public static OperationResult Success { get; } = CreateSuccess();

    public static OperationResult NotImplemented { get; } = CreateNotImplemented();

    public static OperationResult Cancel { get; } = CreateCancel();

    public static OperationResult CreateCancel(Exception ex, TimeSpan elapsed = default) => new(OperationResultCodes.Canceled, ex, elapsed);

    public static OperationResult CreateCancel(TimeSpan elapsed = default) => CreateError(OperationResultCodes.Canceled, "the operation was canceled", elapsed);

    public static OperationResult CreateSuccess(TimeSpan elapsed = default) => new(default, elapsed);

    public static OperationResult CreateError(int code, Exception ex, TimeSpan elapsed = default) => new(code, ex, elapsed);

    public static OperationResult CreateError(int code, string? error, TimeSpan elapsed = default) => CreateError(code, new SimpleException(error), elapsed);

    public static OperationResult CreateError(string? error, TimeSpan elapsed = default) => CreateError(OperationResultCodes.StringError, error, elapsed);

    public static OperationResult CreateError(Exception ex, TimeSpan elapsed = default)
    {
        return new(IsCancelException(ex) ? OperationResultCodes.Canceled : OperationResultCodes.Exception, ex, elapsed);
    }

    public static OperationResult CreateNotImplemented() => CreateError(OperationResultCodes.NotImplemented, "the operation was not implemented", default);
}