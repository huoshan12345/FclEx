using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace FclEx.Abp.Caching
{
    public class AbpDistributedCache : IDistributedCache
    {
        public const string CacheNameOfValue = "DistributedCache:Value";
        public const string CacheNameOfExpiration = "DistributedCache:Expiration";

        protected readonly ICacheManager _cacheManager;
        protected readonly Lazy<ICache<byte[]>> _cache;
        protected readonly Lazy<ICache<TimeSpan>> _cacheOfExpiration;
        protected ICache<byte[]> Cache => _cache.Value;
        protected ICache<TimeSpan> CacheOfExpiration => _cacheOfExpiration.Value;

        public AbpDistributedCache(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
            _cache = new Lazy<ICache<byte[]>>(() => _cacheManager.GetCache<byte[]>(CacheNameOfValue), true);
            _cacheOfExpiration = new Lazy<ICache<TimeSpan>>(() => _cacheManager.GetCache<TimeSpan>(CacheNameOfExpiration), true);
        }

        public byte[] Get(string key)
        {
            return Cache.Get(key).Value;
        }

        public async Task<byte[]> GetAsync(string key, CancellationToken token = new CancellationToken())
        {
            var v = await Cache.GetAsync(key).DonotCapture();
            return v.Value;
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            var exp = GetExpiration(options);
            Cache.Set(key, value, exp);
            if (exp is { } timeSpan)
                CacheOfExpiration.Set(key, timeSpan, timeSpan);
        }

        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options,
            CancellationToken token = new CancellationToken())
        {
            var exp = GetExpiration(options);
            await Cache.SetAsync(key, value, exp).DonotCapture();
            if (exp is { } timeSpan)
                await CacheOfExpiration.SetAsync(key, timeSpan, timeSpan).DonotCapture();
        }

        public void Refresh(string key)
        {
            if (CacheOfExpiration.TryGet(key, out var timeSpan)
                && Cache.TryGet(key, out var bytes))
            {
                Cache.Set(key, bytes, timeSpan);
                CacheOfExpiration.Set(key, timeSpan, timeSpan);
            }
        }

        public async Task RefreshAsync(string key, CancellationToken token = new CancellationToken())
        {
            if (CacheOfExpiration.TryGet(key, out var timeSpan)
                && Cache.TryGet(key, out var bytes))
            {
                await Cache.SetAsync(key, bytes, timeSpan).DonotCapture();
                await CacheOfExpiration.SetAsync(key, timeSpan, timeSpan).DonotCapture();
            }
        }

        public void Remove(string key)
        {
            Cache.Remove(key);
            CacheOfExpiration.Remove(key);
        }

        public async Task RemoveAsync(string key, CancellationToken token = new CancellationToken())
        {
            await Cache.RemoveAsync(key).DonotCapture();
            await CacheOfExpiration.RemoveAsync(key).DonotCapture();
        }

        private static TimeSpan? GetExpiration(DistributedCacheEntryOptions? options)
        {
            if (options == null)
                return null;
            if (options.SlidingExpiration is { } slidingExpiration)
                return slidingExpiration;
            if (options.AbsoluteExpiration is { } absoluteExpiration)
                return absoluteExpiration - DateTimeOffset.UtcNow;
            if (options.AbsoluteExpirationRelativeToNow is { } absoluteExpirationRelativeToNow)
                return absoluteExpirationRelativeToNow;
            return null;
        }
    }
}
