using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using EasyCaching.Core;

namespace FclEx.Utils;

public static class FclExAbpExtensions
{
    public static OperateResult<T> Unwrap<T>(this OperateResult<CacheValue<T>> result)
    {
        if (result.Success)
        {
            var value = result.Value;
            if (value.HasValue)
            {
                return (value.Value, result.Elapsed);
            }
            else
            {
                return ("Failed to get value from the cache", result.Elapsed);
            }
        }
        else
        {
            return result.ToExplicit<T>();
        }
    }

    public static async Task<OperateResult<T>> Unwrap<T>(this Task<OperateResult<CacheValue<T>>> result)
    {
        return (await result.IgnoreSyncContext()).Unwrap();
    }
}