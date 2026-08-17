namespace FclEx.Caching;

public static class CacheManagerExtensions
{
    public static Task<OperationResult<T>> GetOrCreateAsync<T>(
        this ICacheManager cacheManager,
        string cacheName,
        string cacheKey,
        Func<Task<OperationResult<T>>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        return Operation.ExecuteAsync(t =>
        {
            var cache = cacheManager.GetCache<T>(cacheName);
            return cache.TryGet(cacheKey, out var obj)
                ? Operation.Success(obj)
                : factory().OnValue(v => cache.TrySet(cacheKey, v, expiration));
        }, cancellationToken: cancellationToken);
    }

    public static Task<OperationResult<T>> GetOrCreateAsync<T>(
        this ICacheManager cacheManager,
        string cacheName,
        string cacheKey,
        Func<Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        return cacheManager.GetOrCreateAsync(cacheName, cacheKey, () => Operation.ExecuteAsync(t => factory(), cancellationToken: cancellationToken), expiration, cancellationToken);
    }

    public static Task<OperationResult<T>> SetAsync<T>(
        this ICacheManager cacheManager,
        string cacheName,
        string cacheKey,
        Func<Task<OperationResult<T>>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        return Operation.ExecuteAsync(t =>
        {
            var cache = cacheManager.GetCache<T>(cacheName);
            var result = factory().OnValue(v => cache.TrySet(cacheKey, v, expiration));
            return result;
        }, cancellationToken: cancellationToken);
    }

    public static Task<OperationResult<T>> SetAsync<T>(
        this ICacheManager cacheManager,
        string cacheName,
        string cacheKey,
        Func<Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        return cacheManager.SetAsync(cacheName, cacheKey, () => Operation.ExecuteAsync(t => factory(), cancellationToken: cancellationToken), expiration, cancellationToken);
    }
}