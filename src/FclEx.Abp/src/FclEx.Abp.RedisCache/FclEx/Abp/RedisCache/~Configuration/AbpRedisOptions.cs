using System;
using EasyCaching.Redis;

namespace FclEx.Abp.RedisCache;

public class AbpRedisOptions
{
    public string? SerializerName { get; set; } = "json";
    public RedisDBOptions RedisOptions { get; set; } = new();
    public List<RedisCollectionConfigurator> CollectionConfigurators { get; } = [];

    public AbpRedisOptions ConfigureCollection(string name, Action<RedisCollectionOptions> action)
    {
        CollectionConfigurators.Add(new RedisCollectionConfigurator(name, action));
        return this;
    }

    public AbpRedisOptions ConfigureAllCollections(Action<RedisCollectionOptions> action)
    {
        CollectionConfigurators.Add(new RedisCollectionConfigurator(action));
        return this;
    }
}