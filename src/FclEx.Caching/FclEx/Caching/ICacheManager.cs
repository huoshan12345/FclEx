namespace FclEx.Caching;

public interface ICacheManager : IDisposable
{
    IReadOnlyCacheManagerOptions Options { get; }
    ProviderInfo ProviderInfo { get; }
    IReadOnlyList <ICache> GetAllCaches();
    ICache<T> GetCache<T>(string name);
    ICache GetCache(string name);
}