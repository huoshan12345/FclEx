using System;
using System.Collections.Generic;
using System.Text;
using FclEx.Utils;

namespace FclEx.Abp.Caching;

public static class CacheExtensions
{
    public static bool TryGet<T>(this ICache<T> cache, string key, [NotNullWhen(true)] out T? item)
    {
        item = default;
        var (success, value, _, _) = Operate.Execute(() => cache.Get(key));
        if (!success) return false;
        item = value!.Value;
        return value.HasValue;
    }

    public static bool TrySet<T>(this ICache<T> cache, string key, T item, TimeSpan? expiration = null)
    {
        var result = Operate.Execute(() => cache.Set(key, item, expiration));
        return result.Success;
    }
}