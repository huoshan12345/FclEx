using System.Collections.Generic;
using FclEx.Abp.Caching.Configuration;
using FclEx.Abp.RedisCache.Collections;
using FclEx.Abp.RedisCache.Configuration;

namespace FclEx.Abp.RedisCache;

public interface IRedisColManager
{
    IAbpCacheReadOnlyOptions CacheOptions { get; }
    IAbpRedisReadOnlyOptions RedisOptions { get; }
    IReadOnlyList<IRedisCol> GetAllCaches();
    IRedisList<T> GetList<T>(string name) where T : notnull;
    IRedisSet<T> GetSet<T>(string name) where T : notnull;
    IRedisHash<T> GetHash<T>(string name) where T : notnull;
    IRedisSortedSet<T> GetSortedSet<T>(string name) where T : notnull;
}