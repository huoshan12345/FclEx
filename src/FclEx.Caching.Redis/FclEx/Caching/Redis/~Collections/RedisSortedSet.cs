namespace FclEx.Caching.Redis;

internal class RedisSortedSet<T> : RedisCollection<T>, IRedisSortedSet<T> where T : notnull
{
    public RedisSortedSet(string name, IRedisCachingProvider provider, CacheManagerOptions managerOptions)
        : base(name, provider, managerOptions)
    {
    }

    public override RedisCollectionType CollectionType { get; } = RedisCollectionType.SortedSet;

    public Task<long> ZCardAsync() => _provider.ZCardAsync(Key);
    public Task<long> ZCountAsync(double min, double max) => _provider.ZCountAsync(Key, min, max);
    public Task<long> ZLexCountAsync(string min, string max) => _provider.ZLexCountAsync(Key, min, max);
    public Task<long> ZAddAsync(Dictionary<T, double> cacheValues) => _provider.ZAddAsync<T>(Key, cacheValues);
    public Task<List<T>> ZRangeAsync(long start, long stop) => _provider.ZRangeAsync<T>(Key, start, stop);
    public Task<long?> ZRankAsync(T cacheValue) => _provider.ZRankAsync<T>(Key, cacheValue);
    public Task<long> ZRemAsync(IList<T> cacheValues) => _provider.ZRemAsync<T>(Key, cacheValues);
    public Task<double?> ZScoreAsync(T cacheValue) => _provider.ZScoreAsync<T>(Key, cacheValue);
}