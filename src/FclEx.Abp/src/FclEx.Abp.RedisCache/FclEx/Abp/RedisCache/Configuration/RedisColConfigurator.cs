using System;

namespace FclEx.Abp.RedisCache.Configuration;

internal class RedisColConfigurator : IRedisColConfigurator
{
    public RedisColConfigurator(Action<RedisColOptions> initAction) : this("", initAction)
    {
    }

    public RedisColConfigurator(string name, Action<RedisColOptions> action)
    {
        CacheName = name;
        InitAction = action;
    }

    public string CacheName { get; }
    public Action<RedisColOptions> InitAction { get; }
}