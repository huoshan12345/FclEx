namespace FclEx.Caching.Redis;

internal class RedisSet<T> : RedisCollection<T>, IRedisSet<T>
{
    public RedisSet(string name, IRedisCachingProvider provider, CacheManagerOptions managerOptions)
        : base(name, provider, managerOptions)
    {
    }

    public Task<long> SCardAsync() => _provider.SCardAsync(Key);
    public Task<long> SAddAsync(IList<T> cacheValues) => _provider.SAddAsync<T>(Key, cacheValues);
    public Task<bool> SIsMemberAsync(T cacheValue) => _provider.SIsMemberAsync<T>(Key, cacheValue);
    public Task<List<T>> SMembersAsync() => _provider.SMembersAsync<T>(Key);
    public Task<T> SPopAsync() => _provider.SPopAsync<T>(Key);
    public Task<List<T>> SRandMemberAsync(int count = 1) => _provider.SRandMemberAsync<T>(Key, count);
    public Task<long> SRemAsync(IList<T>? cacheValues = null) => _provider.SRemAsync<T>(Key, cacheValues);
    public override RedisCollectionType CollectionType { get; } = RedisCollectionType.Set;
}