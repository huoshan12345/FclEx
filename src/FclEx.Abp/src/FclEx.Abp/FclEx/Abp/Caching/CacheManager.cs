using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using EasyCaching.Core;
using FclEx.Abp.Caching.Configuration;
using Microsoft.Extensions.Options;

namespace FclEx.Abp.Caching
{
    using FclEx.Extensions;

    public class CacheManager : ICacheManager
    {
        protected readonly AbpCacheOptions _options;
        protected readonly IEasyCachingProvider _provider;
        protected readonly ConcurrentDictionary<string, ICache> _caches = new();

        public CacheManager(IEasyCachingProvider provider,
            IOptions<AbpCacheOptions> options)
        {
            _provider = provider;
            _options = options.Value;
        }

        public void Dispose()
        {
            _caches.Clear();
        }

        public IAbpCacheReadOnlyOptions CacheOptions => _options;

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

        protected virtual ICache<T> CreateCache<T>(string name)
        {
            var cache = new Cache<T>(name, _provider, _options);
            var configurators = _options.Configurators.Where(c => c.CacheName.IsNullOrEmpty()
                                                                       || c.CacheName == name).ToArray();
            foreach (var configurator in configurators)
            {
                configurator.InitAction?.Invoke(cache.Options);
            }
            return cache;
        }
    }
}
