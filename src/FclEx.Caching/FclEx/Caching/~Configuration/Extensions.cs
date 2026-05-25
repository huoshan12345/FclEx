namespace FclEx.Caching;

public static class Extensions
{
    public static CacheManagerOptions SetCacheExpireTime(this CacheManagerOptions configuration, string name, TimeSpan timeSpan)
    {
        configuration.Configure(name, o => o.DefaultExpiration = timeSpan);
        return configuration;
    }
}