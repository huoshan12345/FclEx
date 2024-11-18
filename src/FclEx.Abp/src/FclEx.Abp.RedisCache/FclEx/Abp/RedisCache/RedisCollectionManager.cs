using System;

namespace FclEx.Abp.RedisCache;

using FclEx.Extensions;

public class RedisCollectionManager : IRedisCollectionManager, IDisposable
{
    protected readonly ConcurrentDictionary<string, IRedisCol> _caches;
    protected readonly IRedisCachingProvider _provider;
    protected readonly IStringSerializer _stringSerializer;
    protected readonly AbpRedisOptions _abpRedisOptions;
    protected readonly AbpCacheOptions _abpCacheOptions;

    public RedisCollectionManager(
        IRedisCachingProvider provider,
        IStringSerializer stringSerializer,
        IOptions<AbpRedisOptions> abpRedisOptions,
        IOptions<AbpCacheOptions> abpCacheOptions)
    {
        _provider = provider;
        _stringSerializer = stringSerializer;
        _abpCacheOptions = abpCacheOptions.Value;
        _abpRedisOptions = abpRedisOptions.Value;
        _caches = new ConcurrentDictionary<string, IRedisCol>();
    }

    public void Dispose()
    {
        _caches.Clear();
    }

    protected TCol GetCol<T, TCol>(string name, RedisCollectionType RedisCollectionType) where T : notnull

        where TCol : IRedisCollection<T>
    {
        Check.NotNull(name);
        var obj = _caches.GetOrAdd(name, k => CreateAndInitCol<T, TCol>(k, RedisCollectionType));
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

    protected IRedisCollection<T> CreateCol<T>(string name, RedisCollectionType type) where T : notnull
    {
        return type switch
        {
            RedisCollectionType.List => new RedisList<T>(name, _provider, _abpCacheOptions),
            RedisCollectionType.Set => new RedisSet<T>(name, _provider, _abpCacheOptions),
            RedisCollectionType.SortedSet => new RedisSortedSet<T>(name, _provider, _abpCacheOptions),
            RedisCollectionType.Hash => new RedisHash<T>(name, _provider, _abpCacheOptions, _stringSerializer),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    protected TCol CreateAndInitCol<T, TCol>(string name, RedisCollectionType type) where T : notnull

        where TCol : IRedisCollection<T>
    {
        var cache = CreateCol<T>(name, type);
        return (TCol)InitCol(cache);
    }

    protected IRedisCollection<T> InitCol<T>(IRedisCollection<T> Collection)
    {
        var redisCol = (RedisCollection<T>)Collection;
        var configurators = _abpRedisOptions.ColConfigurators.Where(c => c.Name.IsNullOrEmpty()
                                                                      || c.Name == Collection.Name).ToArray();
        foreach (var configurator in configurators)
        {
            configurator.Action?.Invoke(redisCol.Options);
        }
        return Collection;
    }

    public IAbpCacheReadOnlyOptions CacheOptions => _abpCacheOptions;
    public IAbpRedisReadOnlyOptions RedisOptions => _abpRedisOptions;

    public IReadOnlyList<IRedisCol> GetAllCaches()
    {
        return _caches.Values.ToList();
    }

    public IRedisList<T> GetList<T>(string name) where T : notnull
    {
        return GetCol<T, IRedisList<T>>(name, RedisCollectionType.List);
    }

    public IRedisSet<T> GetSet<T>(string name) where T : notnull
    {
        return GetCol<T, IRedisSet<T>>(name, RedisCollectionType.Set);
    }

    public IRedisSortedSet<T> GetSortedSet<T>(string name) where T : notnull
    {
        return GetCol<T, IRedisSortedSet<T>>(name, RedisCollectionType.SortedSet);
    }

    public IRedisHash<T> GetHash<T>(string name) where T : notnull
    {
        return GetCol<T, IRedisHash<T>>(name, RedisCollectionType.Hash);
    }
}