namespace FclEx.Caching.Redis;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFclExCachingWithRedis(
        this IServiceCollection services,
        RedisOptions options,
        Action<CacheManagerOptions>? configureCacheManager = null)
    {
        services.AddOptions();
        services.AddOptionsInstance(options);
        services.AddSingleton<IStringSerializer>(SerializerPresets.StringOrJson);
        services.AddSingleton<IRedisManager, RedisManager>();

        services.AddFclExCaching(configureCacheManager, s =>
        {
            s.UsePatchedRedis(c =>
            {
                c.SerializerName = options.SerializerName;
                c.DBConfig = options.DbOptions;
            }).WithPatchedSystemTextJson();
        });
        return services;
    }
}
