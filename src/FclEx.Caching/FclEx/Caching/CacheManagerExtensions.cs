namespace FclEx.Caching;

public static class CacheManagerExtensions
{
    public static Task<OperationResult<T>> GetOrCreateAsync<T>(
        this ICacheManager cacheManager,
        string cacheName,
        string cacheKey,
        Func<Task<OperationResult<T>>> factory,
        TimeSpan? expiration = null)
    {
        return Operation.ExecuteAsync(() =>
        {
            var cache = cacheManager.GetCache<T>(cacheName);
            return cache.TryGet(cacheKey, out var obj)
                ? Operation.Success(obj)
                : factory().OnValue((o, t) => cache.TrySet(cacheKey, o, expiration));
        });
    }

    public static Task<OperationResult<T>> GetOrCreateAsync<T>(
        this ICacheManager cacheManager,
        string cacheName,
        string cacheKey,
        Func<Task<T>> factory,
        TimeSpan? expiration = null)
    {
        return cacheManager.GetOrCreateAsync(cacheName, cacheKey, () => Operation.ExecuteAsync(factory), expiration);
    }

    public static Task<OperationResult<T>> SetAsync<T>(
        this ICacheManager cacheManager,
        string cacheName,
        string cacheKey,
        Func<Task<OperationResult<T>>> factory,
        TimeSpan? expiration = null)
    {
        return Operation.ExecuteAsync(() =>
        {
            var cache = cacheManager.GetCache<T>(cacheName);
            var result = factory().OnValue((o, t) => cache.TrySet(cacheKey, o, expiration));
            return result;
        });
    }

    public static Task<OperationResult<T>> SetAsync<T>(
        this ICacheManager cacheManager,
        string cacheName,
        string cacheKey,
        Func<Task<T>> factory,
        TimeSpan? expiration = null)
    {
        return cacheManager.SetAsync(cacheName, cacheKey, () => Operation.ExecuteAsync(factory), expiration);
    }
}