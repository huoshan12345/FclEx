using EasyCaching.Core;

namespace FclEx.Utils;

public static class FclExAbpExtensions
{
    public static OperationResult<T> Unwrap<T>(this OperationResult<CacheValue<T>> result)
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

    public static async Task<OperationResult<T>> Unwrap<T>(this Task<OperationResult<CacheValue<T>>> result)
    {
        return (await result.IgnoreSyncContext()).Unwrap();
    }
}