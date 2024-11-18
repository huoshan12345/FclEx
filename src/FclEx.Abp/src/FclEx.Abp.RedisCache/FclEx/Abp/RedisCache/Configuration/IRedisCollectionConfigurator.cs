using System;

namespace FclEx.Abp.RedisCache.Configuration;

public interface IRedisCollectionConfigurator
{
    string Name { get; }
    Action<RedisCollectionOptions> Action { get; }
}