using System;
using EasyCaching.Core.Configurations;

namespace EasyCaching.Redis;

public static class EasyCachingOptionsExtensions
{
    public static EasyCachingOptions UsePatchedRedis(this EasyCachingOptions options, Action<RedisOptions> configure, string name = EasyCachingConstValue.DefaultRedisName)
    {
        ArgumentCheck.NotNull(configure, nameof(configure));

        options.RegisterExtension(new PatchedRedisOptionsExtension(name, configure));
        return options;
    }
}