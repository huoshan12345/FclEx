namespace FclEx.Caching;

public sealed class CacheManager : ICacheManager
{
    private readonly CacheManagerOptions _options;
    private readonly IEasyCachingProvider _provider;
    private readonly ConcurrentDictionary<string, ICache> _caches = new();

    public ProviderInfo ProviderInfo => _provider.GetProviderInfo();
    public IReadOnlyCacheManagerOptions Options => _options;

    public CacheManager(
        IEasyCachingProvider provider,
        IOptions<CacheManagerOptions> options)
    {
        _provider = provider;
        _options = options.Value;
    }

    public void Dispose()
    {
        _caches.Clear();
    }

    public IReadOnlyList<ICache> GetAllCaches()
    {
        return _caches.Values.ToList();
    }

    public ICache<T> GetCache<T>(string name)
    {
        Check.NotNull(name);
        var obj = _caches.GetOrAdd(name, CreateCache<T>);
        if (obj.GetType().GenericTypeArguments.FirstOrDefault() is var t && t != typeof(T))
        {
            throw new ArgumentException($"the type of cache ({t}) is not the same as query type ({typeof(T)})");
        }
        return (ICache<T>)obj;
    }

    public ICache GetCache(string name)
    {
        Check.NotNull(name);

        return _caches.TryGetValue(name, out var cache)
            ? cache
            : throw new InvalidOperationException($"the cache with name({name}) does not exist, you must add it first.");
    }

    private Cache<T> CreateCache<T>(string name)
    {
        var cache = new Cache<T>(name, _provider, _options);
        var configurators = _options.Configurators.Where(c => c.CacheName.IsNullOrEmpty()
                                                              || c.CacheName == name).ToArray();
        foreach (var configurator in configurators)
        {
            cache.Configure(configurator.Action);
        }
        return cache;
    }
}