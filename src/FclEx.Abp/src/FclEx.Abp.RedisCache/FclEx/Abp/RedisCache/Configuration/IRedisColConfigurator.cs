using System;

namespace FclEx.Abp.RedisCache.Configuration
{
    public interface IRedisColConfigurator
    {
        string CacheName { get; }
        Action<RedisColOptions> InitAction { get; }
    }
}
