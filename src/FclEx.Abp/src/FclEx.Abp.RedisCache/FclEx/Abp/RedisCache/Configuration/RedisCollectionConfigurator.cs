using System;

namespace FclEx.Abp.RedisCache.Configuration;

public record RedisCollectionConfigurator(string Name, Action<RedisCollectionOptions> Action)
{
    public RedisCollectionConfigurator(Action<RedisCollectionOptions> initAction) : this("", initAction)
    {
    }
}