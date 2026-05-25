namespace FclEx.Redis;

internal abstract class RedisCollection<T> : IRedisCollection<T>
{
    private readonly Lazy<string> _key;
    protected readonly IRedisCachingProvider _provider;
    protected readonly CacheManagerOptions _managerOptions;
    private readonly RedisCollectionOptions _options;

    public TimeSpan DefaultExpiration => _options.DefaultExpiration ?? _managerOptions.DefaultExpiration;
    public string Name { get; }
    public string Key => _key.Value;
    public abstract RedisCollectionType CollectionType { get; }
    
    protected RedisCollection(
        string name,
        IRedisCachingProvider provider,
        CacheManagerOptions managerOptions)
    {
        _provider = provider;
        _managerOptions = managerOptions;
        _options = new RedisCollectionOptions(name);
        _key = new Lazy<string>(GetKey, true);
        Name = Check.NotNull(name);
    }

    protected virtual string GetKey()
    {
        var key = _options.Name;
        if (_options.UseGlobalPrefix)
            key = _managerOptions.GlobalPrefix + key;
        return key;
    }

    public virtual void Configure(Action<RedisCollectionOptions> action)
    {
        action.Invoke(_options);
    }
}