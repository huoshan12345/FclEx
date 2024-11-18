namespace FclEx.Abp.RedisCache;

public interface IRedisCollectionManager
{
    IAbpCacheReadOnlyOptions CacheOptions { get; }
    IAbpRedisReadOnlyOptions RedisOptions { get; }
    IReadOnlyList<IRedisCol> GetAllCaches();
    IRedisList<T> GetList<T>(string name) where T : notnull;
    IRedisSet<T> GetSet<T>(string name) where T : notnull;
    IRedisHash<T> GetHash<T>(string name) where T : notnull;
    IRedisSortedSet<T> GetSortedSet<T>(string name) where T : notnull;
}