using System;
using FclEx.Abp.Caching.Configuration;
using FclEx.Extensions;
using FclEx.Utils;
using Microsoft.Extensions.Logging;
using Volo.Abp.Modularity;

namespace FclEx.Abp.Caching;

public static class CacheManagerExtensions
{
    public static Task<OperateResult<T>> GetObjectAsync<T>(this ICacheManager cacheManager, Func<Task<OperateResult<T>>> rawGetter,
        string cacheKey, string cacheName, TimeSpan? expiration = null)
    {
        return Operate.ExecuteAsync(() =>
        {
            var cache = cacheManager.GetCache<T>(cacheName);
            return cache.TryGet(cacheKey, out var obj)
                ? Operate.CreateSuccess(obj).ToTask()
                : rawGetter().Ok((o, t) => cache.TrySet(cacheKey, o, expiration));
        });
    }

    public static Task<OperateResult<T>> GetObjectAsync<T>(this ICacheManager cacheManager, Func<Task<T>> rawGetter,
        string cacheKey, string cacheName, TimeSpan? expiration = null)
    {
        return cacheManager.GetObjectAsync(() => Operate.ExecuteAsync(rawGetter), cacheKey, cacheName, expiration);
    }

    public static Task<OperateResult<T>> SetObjectAsync<T>(this ICacheManager cacheManager, Func<Task<OperateResult<T>>> rawGetter,
        string cacheKey, string cacheName, TimeSpan? expiration = null)
    {
        return Operate.ExecuteAsync(() =>
        {
            var cache = cacheManager.GetCache<T>(cacheName);
            var result = rawGetter().Ok((o, t) => cache.TrySet(cacheKey, o, expiration));
            return result;
        });
    }

    public static Task<OperateResult<T>> SetObjectAsync<T>(this ICacheManager cacheManager, Func<Task<T>> rawGetter,
        string cacheKey, string cacheName, TimeSpan? expiration = null)
    {
        return cacheManager.SetObjectAsync(() => Operate.ExecuteAsync(rawGetter), cacheKey, cacheName, expiration);
    }
}