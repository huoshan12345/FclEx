using System;

namespace FclEx.Utils;

public partial class Operate
{
    public static OperateResult<T> CreateNotImplemented<T>() => CreateError<T>(OperateResultCodes.NotImplemented, "the operate was not implemented", default);

    public static OperateResult<T> CreateCancel<T>(Exception ex, TimeSpan elapsed = default) => new(OperateResultCodes.Canceled, ex, elapsed);

    public static OperateResult<T> CreateCancel<T>(TimeSpan elapsed = default) => CreateError<T>(OperateResultCodes.Canceled, "the operate was canceled", elapsed);

    public static OperateResult<T> CreateSuccess<T>(T item, TimeSpan elapsed = default) => new(item!, elapsed);

    public static OperateResult<T> CreateError<T>(int code, string? error, TimeSpan elapsed = default) => new(code, new SimpleException(error), elapsed);

    public static OperateResult<T> CreateError<T>(string? error, TimeSpan elapsed = default) => CreateError<T>(OperateResultCodes.StringError, error, elapsed);

    public static OperateResult<T> CreateError<T>(Exception ex, TimeSpan elapsed = default)
    {
        return new OperateResult<T>(IsCancelException(ex) ? OperateResultCodes.Canceled : OperateResultCodes.Exception, ex, elapsed);
    }
}