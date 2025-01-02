using System;
using FclEx.Abp.Caching;
using FclEx.Domain;

namespace FclEx.Abp.Domain;

public abstract class AbstractKeyValueServiceWithCache : IKeyValueService
{
    protected readonly ICacheManager _cacheManager;

    public AbstractKeyValueServiceWithCache(
        IStringSerializer stringSerializer,
        ICacheManager cacheManager)
    {
        StringSerializer = stringSerializer;
        _cacheManager = cacheManager;
    }

    protected abstract TimeSpan? GetExpiration(string objectId, string key);

    protected virtual ICache<string> GetCache(string objectId)
    {
        return _cacheManager.GetCache<string>(objectId);
    }

    public IStringSerializer StringSerializer { get; }

    protected abstract Task<string?> GetAsyncAction(string objectId, string key, string? defaultValue = default);

    protected abstract Task SaveAsyncAction(string objectId, string key, string? defaultValue = default);

    public virtual async Task<string?> GetAsync(string objectId, string key, string? defaultValue = default)
    {
        var cache = GetCache(objectId);
        var entry = await cache.GetAsync(key).IgnoreSyncContext();
        if (entry.HasValue)
            return entry.Value;

        var value = await GetAsyncAction(objectId, key, defaultValue).IgnoreSyncContext();

        if (value != null)
        {
            var expiration = GetExpiration(objectId, key);
            await cache.SetAsync(key, value, expiration).IgnoreSyncContext();
        }

        return value ?? defaultValue;
    }

    public async Task SaveAsync(string objectId, string key, string? value)
    {
        await SaveAsyncAction(objectId, key, value);

        if (value != null)
        {
            var cache = GetCache(objectId);
            var expiration = GetExpiration(objectId, key);
            await cache.SetAsync(key, value, expiration).IgnoreSyncContext();
        }
    }

    public abstract Task RemoveAsync(string objectId, string key);
}
