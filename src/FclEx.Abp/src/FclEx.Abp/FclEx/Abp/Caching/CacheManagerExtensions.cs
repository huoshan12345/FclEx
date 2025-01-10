using System;

namespace FclEx.Abp.Caching;

public static class CacheManagerExtensions
{
    public static Task<OperationResult<T>> GetObjectAsync<T>(this ICacheManager cacheManager, Func<Task<OperationResult<T>>> rawGetter,
        string cacheKey, string cacheName, TimeSpan? expiration = null)
    {
        return Operation.ExecuteAsync(() =>
        {
            var cache = cacheManager.GetCache<T>(cacheName);
            return cache.TryGet(cacheKey, out var obj)
                ? Operation.CreateSuccess(obj).ToTask()
                : rawGetter().Ok((o, t) => cache.TrySet(cacheKey, o, expiration));
        });
    }

    public static Task<OperationResult<T>> GetObjectAsync<T>(this ICacheManager cacheManager, Func<Task<T>> rawGetter,
        string cacheKey, string cacheName, TimeSpan? expiration = null)
    {
        return cacheManager.GetObjectAsync(() => Operation.ExecuteAsync(rawGetter), cacheKey, cacheName, expiration);
    }

    public static Task<OperationResult<T>> SetObjectAsync<T>(this ICacheManager cacheManager, Func<Task<OperationResult<T>>> rawGetter,
        string cacheKey, string cacheName, TimeSpan? expiration = null)
    {
        return Operation.ExecuteAsync(() =>
        {
            var cache = cacheManager.GetCache<T>(cacheName);
            var result = rawGetter().Ok((o, t) => cache.TrySet(cacheKey, o, expiration));
            return result;
        });
    }

    public static Task<OperationResult<T>> SetObjectAsync<T>(this ICacheManager cacheManager, Func<Task<T>> rawGetter,
        string cacheKey, string cacheName, TimeSpan? expiration = null)
    {
        return cacheManager.SetObjectAsync(() => Operation.ExecuteAsync(rawGetter), cacheKey, cacheName, expiration);
    }
}