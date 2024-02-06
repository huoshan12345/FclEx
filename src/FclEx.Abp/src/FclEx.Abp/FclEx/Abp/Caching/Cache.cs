using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using EasyCaching.Core;
using FclEx.Abp.Caching.Configuration;

namespace FclEx.Abp.Caching;

using FclEx.Extensions;

internal sealed class Cache<T> : ICache<T>
{
    private readonly IEasyCachingProvider _provider;
    private readonly Lazy<string> _prefix;
    private readonly AbpCacheOptions _options;
    internal CacheOptions Options { get; }

    public string Prefix => _prefix.Value;
    public TimeSpan DefaultExpiration => Options.DefaultExpiration ?? _options.DefaultExpiration;
    public Type ProviderType { get; }
    public string Name => Options.Name;

    private string GetPrefix()
    {
        if (!Options.UsePrefix)
            return "";

        var prefix = Options.Name + _options.Separator;
        if (Options.UseGlobalPrefix)
        {
            prefix = _options.GlobalPrefix + _options.Separator + prefix;
        }
        if (Options.OnlyUseLowerCase)
        {
            prefix = prefix.ToLower();
        }
        return prefix;
    }

    private static readonly ConcurrentDictionary<string, string> _keys = new();
    private string GetKey(string key)
    {
        if (key.IsNullOrEmpty())
            return Prefix;

        return _keys.GetOrAdd(key, m =>
        {
            var k = Options.OnlyUseLowerCase
                ? m.ToLower()
                : m;

            return Prefix + k;
        });
    }

    private string TrimKeyPrefix(string key) => key.TrimStart(Prefix);

    public Cache(string name,
        IEasyCachingProvider provider,
        AbpCacheOptions options)
    {
        Check.NotNull(name);
        Check.NotNull(provider);

        _options = options;
        _provider = provider;
        ProviderType = _provider.GetType();
        Options = new CacheOptions(name);
        _prefix = new Lazy<string>(GetPrefix, true);
    }

    public void Remove(string cacheKey) => _provider.Remove(GetKey(cacheKey));

    public Task RemoveAsync(string cacheKey) => _provider.RemoveAsync(GetKey(cacheKey));

    public bool Exists(string cacheKey) => _provider.Exists(GetKey(cacheKey));

    public Task<bool> ExistsAsync(string cacheKey) => _provider.ExistsAsync(GetKey(cacheKey));

    public void RemoveAll(IEnumerable<string> cacheKeys)
        => _provider.RemoveAll(cacheKeys.Select(GetKey));

    public Task RemoveAllAsync(IEnumerable<string> cacheKeys)
        => _provider.RemoveAllAsync(cacheKeys.Select(GetKey));

    public int GetCount() => _provider.GetCount(Prefix);

    public void RemoveAll() => _provider.RemoveByPrefix(Prefix);

    public Task RemoveAllAsync() => _provider.RemoveByPrefixAsync(Prefix);

    public void Set(string cacheKey, T cacheValue, TimeSpan? expiration = null)
        => _provider.Set(GetKey(cacheKey), cacheValue, expiration ?? DefaultExpiration);

    public Task SetAsync(string cacheKey, T cacheValue, TimeSpan? expiration = null)
        => _provider.SetAsync(GetKey(cacheKey), cacheValue, expiration ?? DefaultExpiration);

    public CacheValue<T> Get(string cacheKey, Func<string, T> dataRetriever, TimeSpan? expiration = null)
    {
        var key = GetKey(cacheKey);
        return _provider.Get(key, () => dataRetriever(key), expiration ?? DefaultExpiration);
    }

    public Task<CacheValue<T>> GetAsync(string cacheKey, Func<string, Task<T>> dataRetriever, TimeSpan? expiration = null)
    {
        var key = GetKey(cacheKey);
        return _provider.GetAsync(key, () => dataRetriever(key), expiration ?? DefaultExpiration);
    }

    public CacheValue<T> Get(string cacheKey)
        => _provider.Get<T>(GetKey(cacheKey));

    public Task<CacheValue<T>> GetAsync(string cacheKey)
        => _provider.GetAsync<T>(GetKey(cacheKey));

    public void SetAll(IDictionary<string, T> value, TimeSpan? expiration = null)
        => _provider.SetAll<T>(value.ToDictionary(m => GetKey(m.Key), m => m.Value), expiration ?? DefaultExpiration);

    public Task SetAllAsync(IDictionary<string, T> value, TimeSpan? expiration = null)
        => _provider.SetAllAsync<T>(value.ToDictionary(m => GetKey(m.Key), m => m.Value), expiration ?? DefaultExpiration);

    public IDictionary<string, CacheValue<T>> GetAll(IEnumerable<string> cacheKeys)
    {
        var dic = _provider.GetAll<T>(cacheKeys.Select(GetKey));
        return dic.ToDictionary(m => TrimKeyPrefix(m.Key), m => m.Value);
    }

    public async Task<IDictionary<string, CacheValue<T>>> GetAllAsync(IEnumerable<string> cacheKeys)
    {
        var dic = await _provider.GetAllAsync<T>(cacheKeys.Select(GetKey)).IgnoreSyncContext();
        return dic.ToDictionary(m => TrimKeyPrefix(m.Key), m => m.Value);
    }
}