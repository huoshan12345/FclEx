using EasyCaching.Core.Configurations;

namespace FclEx.Abp.RedisCache;

public interface IReadOnlyAbpRedisOptions
{
    BaseRedisOptions RedisOptions { get; }
    int Database { get; }
    int Timeout { get; }
}