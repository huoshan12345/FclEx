namespace FclEx.Caching;

public interface ICache
{
    string Name { get; }
    string Prefix { get; }
    void Configure(Action<CacheOptions> action);
    void Remove(string cacheKey);
    Task RemoveAsync(string cacheKey, CancellationToken cancellationToken = default);
    bool Exists(string cacheKey);
    Task<bool> ExistsAsync(string cacheKey, CancellationToken cancellationToken = default);
    void RemoveAll(IEnumerable<string> cacheKeys);
    Task RemoveAllAsync(IEnumerable<string> cacheKeys, CancellationToken cancellationToken = default);
    int GetCount();
    void RemoveAll();
    Task RemoveAllAsync(CancellationToken cancellationToken = default);
}

public interface ICache<T> : ICache
{
    void Set(string cacheKey, T cacheValue, TimeSpan? expiration = null);
    Task SetAsync(string cacheKey, T cacheValue, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    CacheValue<T> Get(string cacheKey, Func<string, T> dataRetriever, TimeSpan? expiration = null);
    Task<CacheValue<T>> GetAsync(string cacheKey, Func<string, Task<T>> dataRetriever, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    CacheValue<T> Get(string cacheKey);
    Task<CacheValue<T>> GetAsync(string cacheKey, CancellationToken cancellationToken = default);
    void SetAll(IDictionary<string, T> value, TimeSpan? expiration = null);
    Task SetAllAsync(IDictionary<string, T> value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    IDictionary<string, CacheValue<T>> GetAll(IEnumerable<string> cacheKeys);
    Task<IDictionary<string, CacheValue<T>>> GetAllAsync(IEnumerable<string> cacheKeys, CancellationToken cancellationToken = default);
}