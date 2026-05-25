namespace FclEx.Redis;

public class RedisCollectionManager : IRedisCollectionManager, IDisposable
{
    private readonly ConcurrentDictionary<string, IRedisCollection> _caches;
    private readonly IRedisCachingProvider _provider;
    private readonly IStringSerializer _stringSerializer;
    private readonly RedisOptions _redisOptions;
    private readonly CacheManagerOptions _managerOptions;

    public RedisCollectionManager(
        IRedisCachingProvider provider,
        IStringSerializer stringSerializer,
        IOptions<RedisOptions> redisOptions,
        IOptions<CacheManagerOptions> managerOptions)
    {
        _provider = provider;
        _stringSerializer = stringSerializer;
        _managerOptions = managerOptions.Value;
        _redisOptions = redisOptions.Value;
        _caches = new ConcurrentDictionary<string, IRedisCollection>();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _caches.Clear();
    }

    private TCol GetCollection<T, TCol>(string name, RedisCollectionType redisCollectionType)
        where T : notnull
        where TCol : IRedisCollection<T>
    {
        Check.NotNull(name);
        var obj = _caches.GetOrAdd(name, k => CreateAndInitCol<T, TCol>(k, redisCollectionType));
        var type = obj.GetType();
        var colType = typeof(TCol);

        if (type.GenericTypeArguments.Length != 1)
            throw new ArgumentException($"the element type of cache ({type.ShortName()}) is a generic type of one argument");

        if (!colType.IsAssignableFrom(type))
            throw new ArgumentException($"the type of cache ({type.ShortName()}) is not the type: " + colType.ShortName());

        if (type.GenericTypeArguments.First() is var t && t != typeof(T))
            throw new ArgumentException($"the element type of cache ({t.ShortName()}) is not the same as query type ({typeof(T).ShortName()})");
        return (TCol)obj;
    }

    private IRedisCollection<T> CreateCol<T>(string name, RedisCollectionType type) where T : notnull
    {
        return type switch
        {
            RedisCollectionType.List => new RedisList<T>(name, _provider, _managerOptions),
            RedisCollectionType.Set => new RedisSet<T>(name, _provider, _managerOptions),
            RedisCollectionType.SortedSet => new RedisSortedSet<T>(name, _provider, _managerOptions),
            RedisCollectionType.Hash => new RedisHash<T>(name, _provider, _managerOptions, _stringSerializer),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    private TCol CreateAndInitCol<T, TCol>(string name, RedisCollectionType type) where T : notnull

        where TCol : IRedisCollection<T>
    {
        var cache = CreateCol<T>(name, type);
        return (TCol)InitCol(cache);
    }

    private IRedisCollection<T> InitCol<T>(IRedisCollection<T> collection)
    {
        var redisCol = (RedisCollection<T>)collection;
        var configurators = _redisOptions.CollectionConfigurators.Where(c => c.Name.IsNullOrEmpty()
                                                                      || c.Name == collection.Name).ToArray();
        foreach (var configurator in configurators)
        {
            redisCol.Configure(configurator.Action);
        }
        return collection;
    }

    public IReadOnlyList<IRedisCollection> GetAllCaches()
    {
        return _caches.Values.ToList();
    }

    public IRedisList<T> GetList<T>(string name) where T : notnull
    {
        return GetCollection<T, IRedisList<T>>(name, RedisCollectionType.List);
    }

    public IRedisSet<T> GetSet<T>(string name) where T : notnull
    {
        return GetCollection<T, IRedisSet<T>>(name, RedisCollectionType.Set);
    }

    public IRedisSortedSet<T> GetSortedSet<T>(string name) where T : notnull
    {
        return GetCollection<T, IRedisSortedSet<T>>(name, RedisCollectionType.SortedSet);
    }

    public IRedisHash<T> GetHash<T>(string name) where T : notnull
    {
        return GetCollection<T, IRedisHash<T>>(name, RedisCollectionType.Hash);
    }
}