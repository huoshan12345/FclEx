namespace FclEx.Caching;

public class CacheManagerOptions : IReadOnlyCacheManagerOptions
{
    public char? Separator { get; set; } = ':';
    public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromDays(1);
    public List<ICacheConfigurator> Configurators { get; } = [];

    private readonly Lazy<string> _defaultGlobalPrefix = new(() => (Assembly.GetEntryAssembly() ?? typeof(ICache).Assembly).GetName().Name!.ToLower(), true);

    /// <summary>
    /// default value is lazy and the assembly name will be returned.
    /// </summary>
    public string GlobalPrefix
    {
        get => field ??= _defaultGlobalPrefix.Value;
        set;
    }

    public CacheManagerOptions Configure(string name, Action<CacheOptions> action)
    {
        Configurators.Add(new CacheConfigurator(name, action));
        return this;
    }

    public CacheManagerOptions ConfigureAll(Action<CacheOptions> action)
    {
        Configurators.Add(new CacheConfigurator(action));
        return this;
    }
}