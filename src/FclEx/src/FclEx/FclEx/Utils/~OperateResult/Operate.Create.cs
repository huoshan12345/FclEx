using System.Threading.Tasks;

namespace FclEx.Utils;

public partial class Operate
{
    private static bool IsCancelException(Exception ex)
    {
        var t = ex.GetType();
        var isCancel = t == typeof(TaskCanceledException) || t == typeof(OperationCanceledException);
        return isCancel;
    }

    public static OperateResult Success { get; } = CreateSuccess();

    public static OperateResult NotImplemented { get; } = CreateNotImplemented();

    public static OperateResult Cancel { get; } = CreateCancel();

    public static OperateResult CreateCancel(Exception ex, TimeSpan elapsed = default) => new(OperateResultCodes.Canceled, ex, elapsed);

    public static OperateResult CreateCancel(TimeSpan elapsed = default) => CreateError(OperateResultCodes.Canceled, "the operate was canceled", elapsed);

    public static OperateResult CreateSuccess(TimeSpan elapsed = default) => new(default, elapsed);

    public static OperateResult CreateError(int code, Exception ex, TimeSpan elapsed = default) => new(code, ex, elapsed);

    public static OperateResult CreateError(int code, string? error, TimeSpan elapsed = default) => CreateError(code, new SimpleException(error), elapsed);

    public static OperateResult CreateError(string? error, TimeSpan elapsed = default) => CreateError(OperateResultCodes.StringError, error, elapsed);

    public static OperateResult CreateObjError<T>(T obj, string error, TimeSpan elapsed = default) where T : notnull
    {
        return new(OperateResultCodes.StringError, ObjectException.Create(obj, error), elapsed);
    }

    public static OperateResult CreateError(Exception ex, TimeSpan elapsed = default)
    {
        return new(IsCancelException(ex) ? OperateResultCodes.Canceled : OperateResultCodes.Exception, ex, elapsed);
    }

    public static OperateResult CreateObjError<T>(T obj, Exception ex, TimeSpan elapsed = default) where T : notnull
    {
        return new(IsCancelException(ex) ? OperateResultCodes.Canceled : OperateResultCodes.Exception,
            ObjectException.Create(obj, ex.Message, ex), elapsed);
    }

    public static OperateResult CreateNotImplemented() => CreateError(OperateResultCodes.NotImplemented, "the operate was not implemented", default);
}