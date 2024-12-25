namespace FclEx.Abp.RedisCache;

internal class RedisList<T> : RedisCollection<T>, IRedisList<T>
{
    public RedisList(string name, IRedisCachingProvider provider, AbpCacheOptions options)
        : base(name, provider, options)
    {
    }

    public override RedisCollectionType CollectionType { get; } = RedisCollectionType.List;
    public long LLen() => _provider.LLen(Key);
    public Task<long> LLenAsync() => _provider.LLenAsync(Key);
    public Task<bool> LTrimAsync(long start, long stop) => _provider.LTrimAsync(Key, start, stop);
    public T LIndex(long index) => _provider.LIndex<T>(Key, index);
    public T LPop() => _provider.LPop<T>(Key);
    public long LPush(IList<T> cacheValues) => _provider.LPush<T>(Key, cacheValues);
    public List<T> LRange(long start, long stop) => _provider.LRange<T>(Key, start, stop);
    public long LRem(long count, T cacheValue) => _provider.LRem<T>(Key, count, cacheValue);
    public bool LSet(long index, T cacheValue) => _provider.LSet<T>(Key, index, cacheValue);
    public bool LTrim(long start, long stop) => _provider.LTrim(Key, start, stop);
    public long LPushX(T cacheValue) => _provider.LPushX(Key, cacheValue);
    public long LInsertBefore(T pivot, T cacheValue) => _provider.LInsertBefore(Key, pivot, cacheValue);
    public long LInsertAfter(T pivot, T cacheValue) => _provider.LInsertAfter(Key, pivot, cacheValue);
    public long RPushX(T cacheValue) => _provider.RPushX<T>(Key, cacheValue);
    public long RPush(IList<T> cacheValues) => _provider.RPush<T>(Key, cacheValues);
    public T RPop() => _provider.RPop<T>(Key);
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