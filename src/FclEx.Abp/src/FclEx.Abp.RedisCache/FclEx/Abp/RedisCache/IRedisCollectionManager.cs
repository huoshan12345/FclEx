namespace FclEx.Abp.RedisCache;

public interface IRedisCollectionManager
{
    IReadOnlyCacheOptions CacheOptions { get; }
    IReadOnlyList<IRedisCollection> GetAllCaches();
    IRedisList<T> GetList<T>(string name) where T : notnull;
    IRedisSet<T> GetSet<T>(string name) where T : notnull;
    IRedisHash<T> GetHash<T>(string name) where T : notnull;
    IRedisSortedSet<T> GetSortedSet<T>(string name) where T : notnull;
}