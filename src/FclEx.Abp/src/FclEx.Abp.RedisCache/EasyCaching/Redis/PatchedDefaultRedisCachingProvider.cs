using EasyCaching.Core.Serialization;
using EasyCaching.Redis.DistributedLock;
using StackExchange.Redis;

namespace EasyCaching.Redis;

public class PatchedRedisCachingProvider : DefaultRedisCachingProvider, IRedisCachingProvider
{
    private readonly IDatabase _cache;
    public PatchedRedisCachingProvider(
        string name,
        IEnumerable<IRedisDatabaseProvider> dbProviders,
        IEnumerable<IEasyCachingSerializer> serializers,
        RedisOptions options,
        RedisLockFactory? factory = null,
        ILoggerFactory? loggerFactory = null)
        : base(name, dbProviders, serializers, options, factory, loggerFactory)
    {
        _cache = (IDatabase)Database;
    }

    long IRedisCachingProvider.ZCount(string cacheKey, double min, double max)
    {
        ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var len = _cache.SortedSetLength(cacheKey, min, max);
        return len;
    }

    async Task<long> IRedisCachingProvider.ZCountAsync(string cacheKey, double min, double max)
    {
        ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var len = await _cache.SortedSetLengthAsync(cacheKey, min, max);
        return len;
    }
}