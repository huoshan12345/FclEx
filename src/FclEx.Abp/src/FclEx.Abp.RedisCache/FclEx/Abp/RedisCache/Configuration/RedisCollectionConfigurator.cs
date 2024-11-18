using System;

namespace FclEx.Abp.RedisCache.Configuration;

internal record RedisCollectionConfigurator(string Name, Action<RedisCollectionOptions> Action) : IRedisCollectionConfigurator
{
    public RedisCollectionConfigurator(Action<RedisCollectionOptions> initAction) : this("", initAction)
    {
    }
}