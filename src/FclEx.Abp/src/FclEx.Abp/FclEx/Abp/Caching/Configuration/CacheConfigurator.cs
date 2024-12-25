using System;

namespace FclEx.Abp.Caching.Configuration;

internal class CacheConfigurator : ICacheConfigurator
{
    public string CacheName { get; }
    public Action<CacheOptions> Action { get; }

    public CacheConfigurator(Action<CacheOptions> Action) : this(string.Empty, Action)
    {
    }

    public CacheConfigurator(string cacheName, Action<CacheOptions> action)
    {
        CacheName = cacheName;
        Action = action;
    }
}