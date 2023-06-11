using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace FclEx.Abp.Caching.Configuration;

public class AbpCacheOptions : IAbpCacheReadOnlyOptions
{
    public char? Separator { get; set; } = ':';
    public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromDays(1);

    private readonly Lazy<string> _defaultGlobalPrefix = new(() => (Assembly.GetEntryAssembly() ?? typeof(ICache).Assembly)!.GetName().Name!.ToLower(), true);

    private string? _globalPrefix;
    /// <summary>
    /// default value is lazy and the assembly name will be returned.
    /// </summary>
    public string GlobalPrefix
    {
        get => _globalPrefix ??= _defaultGlobalPrefix.Value;
        set => _globalPrefix = value;
    }

    private readonly List<ICacheConfigurator> _configurators = new();
    public IReadOnlyList<ICacheConfigurator> Configurators => _configurators;

    public AbpCacheOptions Configure(string name, Action<CacheOptions> action)
    {
        _configurators.Add(new CacheConfigurator(name, action));
        return this;
    }

    public AbpCacheOptions ConfigureAll(Action<CacheOptions> action)
    {
        _configurators.Add(new CacheConfigurator(action));
        return this;
    }
}