using EasyCaching.Core.Configurations;

namespace FclEx.Abp.RedisCache.Configuration;

public interface IReadOnlyAbpRedisOptions
{
    BaseRedisOptions RedisOptions { get; }
    int Database { get; }
    int Timeout { get; }
}