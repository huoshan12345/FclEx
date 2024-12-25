using System;
using EasyCaching.Core.Configurations;
using EasyCaching.Core.Serialization;
using EasyCaching.Redis.DistributedLock;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EasyCaching.Redis;

internal class PatchedRedisOptionsExtension : IEasyCachingOptionsExtension
{
    private readonly string _name;
    private readonly Action<RedisOptions> _configure;

    public PatchedRedisOptionsExtension(string name, Action<RedisOptions> configure)
    {
        _name = name;
        _configure = configure;
    }

    public void AddServices(IServiceCollection services)
    {
        services.AddOptions();

        services.Configure(_name, _configure);

        services.TryAddSingleton<IEasyCachingProviderFactory, DefaultEasyCachingProviderFactory>();
        services.AddSingleton<IRedisDatabaseProvider, RedisDatabaseProvider>(x =>
        {
            var optionsMon = x.GetRequiredService<IOptionsMonitor<RedisOptions>>();
            var options = optionsMon.Get(_name);
            return new RedisDatabaseProvider(_name, options);
        });

        Func<IServiceProvider, PatchedRedisCachingProvider> createFactory = x =>
        {
            var dbProviders = x.GetServices<IRedisDatabaseProvider>();
            var serializers = x.GetServices<IEasyCachingSerializer>();
            var optionsMon = x.GetRequiredService<IOptionsMonitor<RedisOptions>>();
            var options = optionsMon.Get(_name);
            var dlf = x.GetService<RedisLockFactory>();
            var factory = x.GetService<ILoggerFactory>();
            return new PatchedRedisCachingProvider(_name, dbProviders, serializers, options, dlf, factory);
        };

        services.AddSingleton<IEasyCachingProvider>(createFactory);
        services.AddSingleton<IRedisCachingProvider>(createFactory);
    }
}