namespace EasyCaching.Core;

public static class CacheValueExtensions
{
    public static OperationResult<T> Unwrap<T>(this OperationResult<CacheValue<T>> result)
    {
        if (result.IsSuccess)
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
            return result.Cast<T>();
        }
    }

    public static Task<OperationResult<T>> Unwrap<T>(this Task<OperationResult<CacheValue<T>>> result)
    {
        return result.Then(m => m.Unwrap());
    }
}
