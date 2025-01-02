using EasyCaching.Redis;
using FclEx.Extensions;

namespace FclEx.Abp.RedisCache;

[DependsOn(typeof(FclExAbpModule))]
public class FclExAbpRedisModule : AbpModule
{
    private AbpRedisOptions? _options;

    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<IRedisCollectionManager, RedisCollectionManager>();
    }

    public override void PostConfigureServices(ServiceConfigurationContext context)
    {
        _options = context.Services.GetOptions<AbpRedisOptions>();

        context.Services.AddEasyCaching(o =>
        {
            o.UsePatchedRedis(c =>
            {
                c.SerializerName = _options.SerializerName;
                c.DBConfig = _options.RedisOptions;
            });
        });
    }

    public override void OnPreApplicationInitialization(ApplicationInitializationContext context)
    {
        var provider = context.ServiceProvider;
        var logger = provider.CreateLogger(GetType());

        var conStrs = _options?.RedisOptions.Endpoints;
        if (conStrs?.Count > 0)
        {
            logger.LogInformation("Redis endpoints: " + conStrs.Select(m => $"{m.Host}:{m.Port}").JoinWith(", "));
        }
        else
        {
            logger.LogWarning("No valid redis connection has been added.");
        }
    }
}