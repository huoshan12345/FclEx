using System.Threading.Tasks;

namespace FclEx.Abp.RedisCache.Collections;

internal class RedisSortedSet<T> : RedisCol<T>, IRedisSortedSet<T> where T : notnull
{
    public RedisSortedSet(string name, IRedisCachingProvider provider, AbpCacheOptions options)
        : base(name, provider, options)
    {
    }

    public override RedisColType ColType { get; } = RedisColType.SortedSet;

    public long ZCard() => _provider.ZCard(Key);
    public long ZCount(double min, double max) => _provider.ZCount(Key, min, max);
    public long ZLexCount(string min, string max) => _provider.ZLexCount(Key, min, max);
    public Task<long> ZCardAsync() => _provider.ZCardAsync(Key);
    public Task<long> ZCountAsync(double min, double max) => _provider.ZCountAsync(Key, min, max);
    public Task<long> ZLexCountAsync(string min, string max) => _provider.ZLexCountAsync(Key, min, max);
    public long ZAdd(Dictionary<T, double> cacheValues) => _provider.ZAdd<T>(Key, cacheValues);
    public List<T> ZRange(long start, long stop) => _provider.ZRange<T>(Key, start, stop);
    public long? ZRank(T cacheValue) => _provider.ZRank<T>(Key, cacheValue);
    public long ZRem(IList<T> cacheValues) => _provider.ZRem<T>(Key, cacheValues);
    public double? ZScore(T cacheValue) => _provider.ZScore<T>(Key, cacheValue);
    public Task<long> ZAddAsync(Dictionary<T, double> cacheValues) => _provider.ZAddAsync<T>(Key, cacheValues);
    public Task<List<T>> ZRangeAsync(long start, long stop) => _provider.ZRangeAsync<T>(Key, start, stop);
    public Task<long?> ZRankAsync(T cacheValue) => _provider.ZRankAsync<T>(Key, cacheValue);
    public Task<long> ZRemAsync(IList<T> cacheValues) => _provider.ZRemAsync<T>(Key, cacheValues);
    public Task<double?> ZScoreAsync(T cacheValue) => _provider.ZScoreAsync<T>(Key, cacheValue);
}