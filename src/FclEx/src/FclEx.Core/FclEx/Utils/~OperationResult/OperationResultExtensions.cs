using static FclEx.Utils.Operation;

namespace FclEx.Utils;

public static partial class OperationResultExtensions
{
    public static void Deconstruct(this OperationResult result, out bool success, out Exception? ex, out TimeSpan elapsed)
    {
        success = result.Success;
        elapsed = result.Elapsed;
        ex = result.Exception;
    }

    [SuppressMessage("ReSharper", "UseDeconstructionOnParameter")]
    public static void Deconstruct(this OperationResult result, out bool success, out Exception? ex)
    {
        success = result.Success;
        ex = result.Exception;
    }
    
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public static OperationResult Merge(this IEnumerable<OperationResult> enumerable)
    {
        Check.NotNull(enumerable);
        
        var time = enumerable.EmptyIfNull().Sum(m => m.Elapsed);
        var exceptions = enumerable.EmptyIfNull().Select(m => m.Exception).NotNull().ToList();
        return exceptions.Count switch
        {
            0 => CreateSuccess(time),
            1 => CreateError(exceptions[0], time),
            _ => CreateError(new AggregateException(exceptions), time)
        };
    }

    public static bool IsObjError<T>(this OperationResult<T> result, Func<T, bool> condition) where T : notnull
    {
        return result.Error && result.Exception.IsObjEx(condition);
    }

    public static bool IsObjError<T>(this IOperationResult result, Func<T, bool> condition) where T : notnull
    {
        return result.Error && result.Exception.IsObjEx(condition);
    }

    public static bool IsStrErr(this IOperationResult result)
    {
        return result.Code == OperationResultCodes.StringError;
    }

    public static bool IsExErr(this IOperationResult result)
    {
        return result.Code == OperationResultCodes.Exception;
    }

    public static bool IsCanceled(this IOperationResult result)
    {
        return result.Code == OperationResultCodes.Canceled;
    }

}