using static FclEx.Utils.Operate;

namespace FclEx.Utils;

public static partial class OperateResultExtensions
{
    public static void Deconstruct(this OperateResult result, out bool success, out Exception? ex, out TimeSpan elapsed)
    {
        success = result.Success;
        elapsed = result.Elapsed;
        ex = result.Exception;
    }

    [SuppressMessage("ReSharper", "UseDeconstructionOnParameter")]
    public static void Deconstruct(this OperateResult result, out bool success, out Exception? ex)
    {
        success = result.Success;
        ex = result.Exception;
    }
    
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public static OperateResult Merge(this IEnumerable<OperateResult> enumerable)
    {
        ArgumentNullException.ThrowIfNull(enumerable);
        
        var time = enumerable.Touch().Sum(m => m.Elapsed);
        var exceptions = enumerable.Touch().Select(m => m.Exception).NotNull().ToList();
        return exceptions.Count switch
        {
            0 => CreateSuccess(time),
            1 => CreateError(exceptions[0], time),
            _ => CreateError(new AggregateException(exceptions), time)
        };
    }

    public static bool IsObjError<T>(this OperateResult<T> result, Func<T, bool> condition) where T : notnull
    {
        return result.Error && result.Exception.IsObjEx(condition);
    }

    public static bool IsObjError<T>(this IOperateResult result, Func<T, bool> condition) where T : notnull
    {
        return result.Error && result.Exception.IsObjEx(condition);
    }

    public static bool IsStrErr(this IOperateResult result)
    {
        return result.Code == OperateResultCodes.StringError;
    }

    public static bool IsExErr(this IOperateResult result)
    {
        return result.Code == OperateResultCodes.Exception;
    }

    public static bool IsCanceled(this IOperateResult result)
    {
        return result.Code == OperateResultCodes.Canceled;
    }

}