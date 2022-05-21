using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FclEx.Extensions;
using static FclEx.Utils.Operate;

namespace FclEx.Utils;

public static partial class OperateResultExtensions
{
    public static void Deconstruct(this OperateResult result, out bool successful, out Exception? ex, out TimeSpan elapsed)
    {
        successful = result.Success;
        elapsed = result.Elapsed;
        ex = result.Exception;
    }

    [SuppressMessage("ReSharper", "UseDeconstructionOnParameter")]
    public static void Deconstruct(this OperateResult result, out bool successful, out Exception? ex)
    {
        successful = result.Success;
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
}