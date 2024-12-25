using System;

namespace FclEx.Abp.RedisCache;

public record RedisCollectionConfigurator(string Name, Action<RedisCollectionOptions> Action)
{
    public RedisCollectionConfigurator(Action<RedisCollectionOptions> initAction) : this("", initAction)
    {
    }
}