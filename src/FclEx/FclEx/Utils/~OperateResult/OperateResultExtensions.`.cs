using System;
using FclEx.Extensions;

namespace FclEx.Utils;

partial class OperateResultExtensions
{
    public static void Deconstruct<T>(this OperateResult<T> result, out bool successful, out T? value, out Exception? ex, out TimeSpan elapsed)
    {
        successful = result.Success;
        ex = result.Exception;
        elapsed = result.Elapsed;
        value = result.Value;
    }

    public static bool IsStrErr<T>(this OperateResult<T> r)
    {
        return r.Code == OperateResultCodes.FromString;
    }

    public static bool IsExErr<T>(this OperateResult<T> r)
    {
        return r.Code == OperateResultCodes.FromException;
    }

    public static bool IsCancelErr<T>(this OperateResult<T> r)
    {
        return r.Code == OperateResultCodes.Cancel;
    }

    public static OperateResult Untype<T>(this OperateResult<T> result) => result;

    public static OperateResult<T> Elapsed<T>(this OperateResult<T> result, TimeSpan span)
    {
        return result.Success
            ? new OperateResult<T>(result.Value, span)
            : new OperateResult<T>(result.Code, result.Exception!, span);
    }

    public static OperateResult<T> ThrowIfError<T>(this OperateResult<T> result)
    {
        if (result.Error)
            result.Exception.ReThrow();
        return result;
    }

    public static OperateResult<T> Unwrap<T>(this OperateResult<OperateResult<T>> result)
    {
        var (successful, innerResult, ex, elapsed) = result;
        return successful
            ? innerResult
            : Operate.CreateError<T>(ex!, elapsed);
    }

    public static OperateResult<TDest> Map<T, TDest>(this OperateResult<T> result, Func<T, TDest> func)
    {
        return result.Success
            ? Operate.CreateSuccess(func(result.Value!))
            : result.ToExplicit<TDest>();
    }

    public static OperateResult<TDest> Bind<T, TDest>(this OperateResult<T> result, Func<T, OperateResult<TDest>> func)
    {
        return result.Success
            ? func(result.Value!)
            : result.ToExplicit<TDest>();
    }
}