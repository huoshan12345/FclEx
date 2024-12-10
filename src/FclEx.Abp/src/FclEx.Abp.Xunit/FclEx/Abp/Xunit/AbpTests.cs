using System;
using Volo.Abp;
using Volo.Abp.Modularity;

namespace FclEx.Abp.Xunit;

public abstract class AbpTests<TModule> where TModule : AbpModule
{
    private readonly Lazy<IServiceProvider> _lazy;
    protected readonly ITestOutputHelper _output;

    protected AbpTests(ITestOutputHelper output)
    {
        _output = output;
        _lazy = new(Initialize);
    }

    public IServiceProvider ServiceProvider => _lazy.Value;

    protected IServiceProvider Initialize()
    {
        var watch = ValueStopwatch.StartNew();

        var config = BuildConfig();
        var services = new ServiceCollection()
            .AddSingleton<IConfigurationRoot>(config)
            .AddSingleton<IConfiguration>(config)
            .AddAbp<TModule>(m => Configure(m, config))
            .AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel);
                builder.AddXunitTest(_output, true);
                builder.AddFilter("Volo.Abp.Modularity.ModuleManager", LogLevel.Warning);
                builder.AddFilter("Volo.Abp.AbpApplicationBase", LogLevel.Warning);
            });

        var provider = UseAbpAsync
            ? SynchronizationContextScope.Run(services.UseAbpAsync)
            : services.UseAbp();

        var logger = provider.CreateLogger("FclEx.Abp.Xunit");
        logger.LogDebug("It takes {ElapsedSeconds} seconds to initialize abp framework", watch.GetElapsedTime().TotalSeconds);
        return provider;
    }

    protected virtual void Configure(AbpApplicationCreationOptions options, IConfigurationRoot configuration)
    {
    }

    protected virtual bool UseAbpAsync { get; } = true;
    protected virtual LogLevel LogLevel => LogLevel.Trace;

    protected virtual IConfigurationRoot BuildConfig()
    {
        return new ConfigurationBuilder().Build();
    }
}