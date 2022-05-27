using System;

using EasyCaching.Core;
using FclEx.Abp.Caching.Configuration;
using FclEx.Abp.RedisCache.Configuration;

namespace FclEx.Abp.RedisCache.Collections
{
    internal abstract class RedisCol<T> : IRedisCol<T>
    {
        private readonly Lazy<string> _key;
        protected readonly IRedisCachingProvider _provider;
        protected readonly AbpCacheOptions _options;
        public TimeSpan DefaultExpiration => Options.DefaultExpiration ?? _options.DefaultExpiration;

        protected RedisCol(string name,
            IRedisCachingProvider provider,
            AbpCacheOptions options)
        {
            _provider = provider;
            _options = options;
            Name = Check.NotNull(name);
            Options = new RedisColOptions(name);
            _key = new Lazy<string>(GetKey, true);
        }

        internal RedisColOptions Options { get; }
        public string Name { get; }
        public string Key => _key.Value;
        public abstract RedisColType ColType { get; }

        protected virtual string GetKey()
        {
            var key = Options.Name;
            if (Options.UseGlobalPrefix)
                key = _options.GlobalPrefix + key;
            return key;
        }
    }
}
