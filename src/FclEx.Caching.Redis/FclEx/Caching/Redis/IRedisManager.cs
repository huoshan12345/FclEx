namespace FclEx.Caching.Redis;

public interface IRedisManager
{
    IReadOnlyList<IRedisCollection> GetAllCaches();
    IRedisList<T> GetList<T>(string name) where T : notnull;
    IRedisSet<T> GetSet<T>(string name) where T : notnull;
    IRedisHash<T> GetHash<T>(string name) where T : notnull;
    IRedisSortedSet<T> GetSortedSet<T>(string name) where T : notnull;
}