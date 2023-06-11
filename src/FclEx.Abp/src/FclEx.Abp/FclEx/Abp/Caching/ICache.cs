using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EasyCaching.Core;

namespace FclEx.Abp.Caching;

public interface ICache
{
    Type ProviderType { get; }
    string Name { get; }
    string Prefix { get; }

    void Remove(string cacheKey);
    Task RemoveAsync(string cacheKey);
    bool Exists(string cacheKey);
    Task<bool> ExistsAsync(string cacheKey);
    void RemoveAll(IEnumerable<string> cacheKeys);
    Task RemoveAllAsync(IEnumerable<string> cacheKeys);
    int GetCount();
    void RemoveAll();
    Task RemoveAllAsync();
}

public interface ICache<T> : ICache
{
    void Set(string cacheKey, T cacheValue, TimeSpan? expiration = null);
    Task SetAsync(string cacheKey, T cacheValue, TimeSpan? expiration = null);
    CacheValue<T> Get(string cacheKey, Func<string, T> dataRetriever, TimeSpan? expiration = null);
    Task<CacheValue<T>> GetAsync(string cacheKey, Func<string, Task<T>> dataRetriever, TimeSpan? expiration = null);
    CacheValue<T> Get(string cacheKey);
    Task<CacheValue<T>> GetAsync(string cacheKey);
    void SetAll(IDictionary<string, T> value, TimeSpan? expiration = null);
    Task SetAllAsync(IDictionary<string, T> value, TimeSpan? expiration = null);
    IDictionary<string, CacheValue<T>> GetAll(IEnumerable<string> cacheKeys);
    Task<IDictionary<string, CacheValue<T>>> GetAllAsync(IEnumerable<string> cacheKeys);
}