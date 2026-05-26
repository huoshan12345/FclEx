using EasyCaching.Serialization.SystemTextJson;
using Microsoft.Extensions.Caching.Distributed;

namespace FclEx.Caching;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFclExCaching(
        this IServiceCollection services,
        Action<CacheManagerOptions>? configureCacheManager = null,
        Action<EasyCachingOptions>? configureEasyCaching = null)
    {
        configureCacheManager ??= _ => { };
        configureEasyCaching ??= o => o.UseInMemory().WithPatchedSystemTextJson();

        return services
            .AddSingleton<ICacheManager, CacheManager>()
            .AddSingleton<IDistributedCache, DistributedCache>()
            .AddEasyCaching(configureEasyCaching)
            .Configure<CacheManagerOptions>(configureCacheManager);
    }
}
