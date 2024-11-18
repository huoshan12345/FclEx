using System;

namespace FclEx.Abp.RedisCache.Collections;

internal abstract class RedisCollection<T> : IRedisCollection<T>
{
    private readonly Lazy<string> _key;
    protected readonly IRedisCachingProvider _provider;
    protected readonly AbpCacheOptions _options;
    public TimeSpan DefaultExpiration => Options.DefaultExpiration ?? _options.DefaultExpiration;

    protected RedisCollection(string name,
        IRedisCachingProvider provider,
        AbpCacheOptions options)
    {
        _provider = provider;
        _options = options;
        Name = Check.NotNull(name);
        Options = new RedisCollectionOptions(name);
        _key = new Lazy<string>(GetKey, true);
    }

    internal RedisCollectionOptions Options { get; }
    public string Name { get; }
    public string Key => _key.Value;
    public abstract RedisCollectionType CollectionType { get; }

    protected virtual string GetKey()
    {
        var key = Options.Name;
        if (Options.UseGlobalPrefix)
            key = _options.GlobalPrefix + key;
        return key;
    }
}