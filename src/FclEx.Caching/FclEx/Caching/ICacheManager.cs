namespace FclEx.Caching;

public interface ICacheManager : IDisposable
{
    IReadOnlyList<ICache> GetAllCaches();
    ICache<T> GetCache<T>(string name);
    ICache GetCache(string name);
    ProviderInfo ProviderInfo { get; }
}