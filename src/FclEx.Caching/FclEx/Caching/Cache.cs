namespace FclEx.Caching;

internal sealed class Cache<T> : ICache<T>
{
    private readonly IEasyCachingProvider _provider;
    private readonly Lazy<string> _prefix;
    private readonly CacheManagerOptions _managerOptions;
    private readonly CacheOptions _options;

    public string Prefix => _prefix.Value;
    public TimeSpan DefaultExpiration => _options.DefaultExpiration ?? _managerOptions.DefaultExpiration;
    public string Name => _options.Name;

    public Cache(
        string name,
        IEasyCachingProvider provider,
        CacheManagerOptions managerOptions)
    {
        Check.NotNull(name);
        Check.NotNull(provider);

        _managerOptions = managerOptions;
        _provider = provider;
        _options = new CacheOptions(name);
        _prefix = new Lazy<string>(GetPrefix, true);
    }

    private string GetPrefix()
    {
        if (!_options.UsePrefix)
            return "";

        var prefix = _options.Name + _managerOptions.Separator;
        if (_options.UseGlobalPrefix)
        {
            prefix = _managerOptions.GlobalPrefix + _managerOptions.Separator + prefix;
        }
        if (_options.OnlyUseLowerCase)
        {
            prefix = prefix.ToLower();
        }
        return prefix;
    }

    // NOTE: cannot make _keys static because it cannot be shared between different cache instances.
    private readonly ConcurrentDictionary<string, string> _keys = new();
    private string GetKey(string key)
    {
        if (key.IsNullOrEmpty())
            return Prefix;

        return _keys.GetOrAdd(key, m =>
        {
            var k = _options.OnlyUseLowerCase
                ? m.ToLower()
                : m;

            return Prefix + k;
        });
    }

    private string TrimKeyPrefix(string key) => key.TrimStart(Prefix);

    public void Configure(Action<CacheOptions> action)
    {
        action.Invoke(_options);
    }

    public void Remove(string cacheKey) => _provider.Remove(GetKey(cacheKey));

    public Task RemoveAsync(string cacheKey, CancellationToken cancellationToken = default) => _provider.RemoveAsync(GetKey(cacheKey), cancellationToken);

    public bool Exists(string cacheKey) => _provider.Exists(GetKey(cacheKey));

    public Task<bool> ExistsAsync(string cacheKey, CancellationToken cancellationToken = default) => _provider.ExistsAsync(GetKey(cacheKey), cancellationToken);

    public void RemoveAll(IEnumerable<string> cacheKeys)
        => _provider.RemoveAll(cacheKeys.Select(GetKey));

    public Task RemoveAllAsync(IEnumerable<string> cacheKeys, CancellationToken cancellationToken = default)
        => _provider.RemoveAllAsync(cacheKeys.Select(GetKey), cancellationToken);

    public int GetCount() => _provider.GetCount(Prefix);

    public void RemoveAll() => _provider.RemoveByPrefix(Prefix);

    public Task RemoveAllAsync(CancellationToken cancellationToken = default) => _provider.RemoveByPrefixAsync(Prefix, cancellationToken);

    public void Set(string cacheKey, T cacheValue, TimeSpan? expiration = null)
        => _provider.Set(GetKey(cacheKey), cacheValue, expiration ?? DefaultExpiration);

    public Task SetAsync(string cacheKey, T cacheValue, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        => _provider.SetAsync(GetKey(cacheKey), cacheValue, expiration ?? DefaultExpiration, cancellationToken);

    public CacheValue<T> Get(string cacheKey, Func<string, T> dataRetriever, TimeSpan? expiration = null)
    {
        var key = GetKey(cacheKey);
        return _provider.Get(key, () => dataRetriever(key), expiration ?? DefaultExpiration);
    }

    public Task<CacheValue<T>> GetAsync(string cacheKey, Func<string, Task<T>> dataRetriever, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var key = GetKey(cacheKey);
        return _provider.GetAsync(key, () => dataRetriever(key), expiration ?? DefaultExpiration, cancellationToken);
    }

    public CacheValue<T> Get(string cacheKey)
        => _provider.Get<T>(GetKey(cacheKey));

    public Task<CacheValue<T>> GetAsync(string cacheKey, CancellationToken cancellationToken = default)
        => _provider.GetAsync<T>(GetKey(cacheKey), cancellationToken);

    public void SetAll(IDictionary<string, T> value, TimeSpan? expiration = null)
        => _provider.SetAll<T>(value.ToDictionary(m => GetKey(m.Key), m => m.Value), expiration ?? DefaultExpiration);

    public Task SetAllAsync(IDictionary<string, T> value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        => _provider.SetAllAsync<T>(value.ToDictionary(m => GetKey(m.Key), m => m.Value), expiration ?? DefaultExpiration, cancellationToken);

    public IDictionary<string, CacheValue<T>> GetAll(IEnumerable<string> cacheKeys)
    {
        var dic = _provider.GetAll<T>(cacheKeys.Select(GetKey));
        return dic.ToDictionary(m => TrimKeyPrefix(m.Key), m => m.Value);
    }

    public async Task<IDictionary<string, CacheValue<T>>> GetAllAsync(IEnumerable<string> cacheKeys, CancellationToken cancellationToken = default)
    {
        var dic = await _provider.GetAllAsync<T>(cacheKeys.Select(GetKey), cancellationToken);
        return dic.ToDictionary(m => TrimKeyPrefix(m.Key), m => m.Value);
    }
}