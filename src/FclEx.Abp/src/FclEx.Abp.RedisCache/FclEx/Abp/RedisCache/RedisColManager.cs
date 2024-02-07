using System;

namespace FclEx.Abp.RedisCache;

using FclEx.Extensions;

public class RedisColManager : IRedisColManager, IDisposable
{
    protected readonly ConcurrentDictionary<string, IRedisCol> _caches;
    protected readonly IRedisCachingProvider _provider;
    protected readonly IStringSerializer _stringSerializer;
    protected readonly AbpRedisOptions _abpRedisOptions;
    protected readonly AbpCacheOptions _abpCacheOptions;

    public RedisColManager(
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

    protected TCol GetCol<T, TCol>(string name, RedisColType redisColType) where T : notnull

        where TCol : IRedisCol<T>
    {
        Check.NotNull(name);
        var obj = _caches.GetOrAdd(name, k => CreateAndInitCol<T, TCol>(k, redisColType));
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

    protected IRedisCol<T> CreateCol<T>(string name, RedisColType type) where T : notnull
    {
        switch (type)
        {
            case RedisColType.List: return new RedisList<T>(name, _provider, _abpCacheOptions);
            case RedisColType.Set: return new RedisSet<T>(name, _provider, _abpCacheOptions);
            case RedisColType.SortedSet: return new RedisSortedSet<T>(name, _provider, _abpCacheOptions);
            case RedisColType.Hash: return new RedisHash<T>(name, _provider, _abpCacheOptions, _stringSerializer);
            default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    protected TCol CreateAndInitCol<T, TCol>(string name, RedisColType type) where T : notnull

        where TCol : IRedisCol<T>
    {
        var cache = CreateCol<T>(name, type);
        return (TCol)InitCol(cache);
    }

    protected IRedisCol<T> InitCol<T>(IRedisCol<T> col)
    {
        var redisCol = (RedisCol<T>)col;
        var configurators = _abpRedisOptions.Configurators.Where(c => c.CacheName.IsNullOrEmpty()
                                                                      || c.CacheName == col.Name).ToArray();
        foreach (var configurator in configurators)
        {
            configurator.InitAction?.Invoke(redisCol.Options);
        }
        return col;
    }

    public IAbpCacheReadOnlyOptions CacheOptions => _abpCacheOptions;
    public IAbpRedisReadOnlyOptions RedisOptions => _abpRedisOptions;

    public IReadOnlyList<IRedisCol> GetAllCaches()
    {
        return _caches.Values.ToList();
    }

    public IRedisList<T> GetList<T>(string name) where T : notnull
    {
        return GetCol<T, IRedisList<T>>(name, RedisColType.List);
    }

    public IRedisSet<T> GetSet<T>(string name) where T : notnull
    {
        return GetCol<T, IRedisSet<T>>(name, RedisColType.Set);
    }

    public IRedisSortedSet<T> GetSortedSet<T>(string name) where T : notnull
    {
        return GetCol<T, IRedisSortedSet<T>>(name, RedisColType.SortedSet);
    }

    public IRedisHash<T> GetHash<T>(string name) where T : notnull
    {
        return GetCol<T, IRedisHash<T>>(name, RedisColType.Hash);
    }
}