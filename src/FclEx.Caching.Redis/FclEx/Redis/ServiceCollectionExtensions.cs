namespace FclEx.Redis;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFclExCachingWithRedis(
        this IServiceCollection services,
        Action<RedisOptions>? configureRedis = null,
        Action<CacheManagerOptions>? configureCacheManager = null)
    {
        var options = new RedisOptions();
        configureRedis?.Invoke(options);

        services.AddOptions();
        services.AddOptionsInstance(options);
        services.AddSingleton<IStringSerializer>(StringAsRawSerializer.Instance);
        services.AddSingleton<IRedisCollectionManager, RedisCollectionManager>();

        services.AddFclExCaching(configureCacheManager, s =>
        {
            s.UsePatchedRedis(c =>
            {
                c.SerializerName = options.SerializerName;
                c.DBConfig = options.DbOptions;
            });
        });
        return services;
    }
}
