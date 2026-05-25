namespace FclEx.Redis;

internal class RedisList<T> : RedisCollection<T>, IRedisList<T>
{
    public RedisList(string name, IRedisCachingProvider provider, CacheManagerOptions managerOptions)
        : base(name, provider, managerOptions)
    {
    }

    public override RedisCollectionType CollectionType { get; } = RedisCollectionType.List;
    public Task<long> LLenAsync() => _provider.LLenAsync(Key);
    public Task<bool> LTrimAsync(long start, long stop) => _provider.LTrimAsync(Key, start, stop);
    public Task<T> LIndexAsync(long index) => _provider.LIndexAsync<T>(Key, index);
    public Task<T> LPopAsync() => _provider.LPopAsync<T>(Key);
    public Task<long> LPushAsync(IList<T> cacheValues) => _provider.LPushAsync<T>(Key, cacheValues);
    public Task<List<T>> LRangeAsync(long start, long stop) => _provider.LRangeAsync<T>(Key, start, stop);
    public Task<long> LRemAsync(long count, T cacheValue) => _provider.LRemAsync<T>(Key, count, cacheValue);
    public Task<bool> LSetAsync(long index, T cacheValue) => _provider.LSetAsync<T>(Key, index, cacheValue);
    public Task<long> LPushXAsync(T cacheValue) => _provider.LPushXAsync<T>(Key, cacheValue);
    public Task<long> LInsertBeforeAsync(T pivot, T cacheValue) => _provider.LInsertBeforeAsync<T>(Key, pivot, cacheValue);
    public Task<long> LInsertAfterAsync(T pivot, T cacheValue) => _provider.LInsertAfterAsync<T>(Key, pivot, cacheValue);
    public Task<long> RPushXAsync(T cacheValue) => _provider.RPushXAsync<T>(Key, cacheValue);
    public Task<long> RPushAsync(IList<T> cacheValues) => _provider.RPushAsync<T>(Key, cacheValues);
    public Task<T> RPopAsync() => _provider.RPopAsync<T>(Key);
}