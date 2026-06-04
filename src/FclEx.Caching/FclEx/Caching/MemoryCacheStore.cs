using Microsoft.Extensions.Caching.Memory;

namespace FclEx.Caching;

public sealed class MemoryCacheStore
{
    public static MemoryCacheStore Shared { get; } = new();

    private readonly MemoryCache _cache;

    public MemoryCacheStore(MemoryCacheOptions? options = null)
    {
        _cache = new MemoryCache(options ?? new());
    }

    public TItem? Get<TItem>(object key)
    {
        return _cache.TryGetValue(key, out TItem? value)
            ? value
            : default;
    }

    public bool TryGetValue<TItem>(object key, out TItem? value)
    {
        return _cache.TryGetValue(key, out value);
    }

    public TItem Set<TItem>(object key, TItem value, MemoryCacheEntryOptions? options = null)
    {
        return _cache.Set(key, value, options);
    }

    public TItem Set<TItem>(object key, TItem value, TimeSpan expiration)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration,
        };
        return Set(key, value, options);
    }

    public TItem GetOrCreate<TItem>(object key, Func<ICacheEntry, TItem> factory, MemoryCacheEntryOptions? options = null)
    {
        return _cache.GetOrCreate(key, factory, options)!;
    }

    public TItem GetOrCreate<TItem>(object key, Func<ICacheEntry, TItem> factory, TimeSpan expiration)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration,
        };
        return GetOrCreate(key, factory, options);
    }

    public Task<TItem> GetOrCreateAsync<TItem>(object key, Func<ICacheEntry, Task<TItem>> factory, MemoryCacheEntryOptions? options = null)
    {
        return _cache.GetOrCreateAsync(key, factory, options)!;
    }

    public Task<TItem> GetOrCreateAsync<TItem>(object key, Func<ICacheEntry, Task<TItem>> factory, TimeSpan expiration)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration,
        };
        return GetOrCreateAsync(key, factory, options);
    }

    public void Remove(object key)
    {
        _cache.Remove(key);
    }

    public void Clear()
    {
        _cache.Clear();
    }
}
