using System;

namespace FclEx.Abp.Caching.Configuration
{
    internal class CacheConfigurator : ICacheConfigurator
    {
        public string CacheName { get; }
        public Action<CacheOptions> InitAction { get; }

        public CacheConfigurator(Action<CacheOptions> initAction) : this(string.Empty, initAction)
        {
        }

        public CacheConfigurator(string cacheName, Action<CacheOptions> initAction)
        {
            CacheName = cacheName;
            InitAction = initAction;
        }
    }
}