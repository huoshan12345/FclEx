using System;
using System.Collections.Generic;
using System.Linq;
using EasyCaching.Core;
using FclEx.Abp.Caching.Configuration;
using Microsoft.Extensions.Options;

namespace FclEx.Abp.Caching;

using FclEx.Extensions;

public sealed class CacheManager : ICacheManager
{
    private readonly AbpCacheOptions _options;
    private readonly IEasyCachingProvider _provider;
    private readonly ConcurrentDictionary<string, ICache> _caches = new();

    public CacheManager(IEasyCachingProvider provider, IOptions<AbpCacheOptions> options)
    {
        _provider = provider;
        _options = options.Value;
    }

    public void Dispose()
    {
        _caches.Clear();
    }

    public IReadOnlyCacheOptions CacheOptions => _options;

    public IReadOnlyList<ICache> GetAllCaches()
    {
        return _caches.Values.ToList();
    }

    public ICache<T> GetCache<T>(string name)
    {
        Check.NotNull(name);
        var obj = _caches.GetOrAdd(name, CreateCache<T>);
        if (obj.GetType().GenericTypeArguments.FirstOrDefault() is var t && t != typeof(T))
        {
            throw new ArgumentException($"the type of cache ({t}) is not the same as query type ({typeof(T)})");
        }
        return (ICache<T>)obj;
    }

    public ICache GetCache(string name)
    {
        Check.NotNull(name);
        if (_caches.TryGetValue(name, out var cache))
            return cache;
        throw new InvalidOperationException($"the cache with name({name}) does not exist, you must add it first.");
    }

    public ProviderInfo ProviderInfo => _provider.GetProviderInfo();

    private Cache<T> CreateCache<T>(string name)
    {
        var cache = new Cache<T>(name, _provider, _options);
        var configurators = _options.Configurators.Where(c => c.CacheName.IsNullOrEmpty()
                                                              || c.CacheName == name).ToArray();
        foreach (var configurator in configurators)
        {
            configurator.Action?.Invoke(cache.Options);
        }
        return cache;
    }
}