namespace FclEx.Caching;

internal class CacheConfigurator(string cacheName, Action<CacheOptions> action) : ICacheConfigurator
{
    public string CacheName { get; } = cacheName;
    public Action<CacheOptions> Action { get; } = action;

    public CacheConfigurator(Action<CacheOptions> action) : this(string.Empty, action)
    {
    }
}