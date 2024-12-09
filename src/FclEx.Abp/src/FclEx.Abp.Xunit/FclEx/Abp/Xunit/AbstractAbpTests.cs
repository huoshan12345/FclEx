using System;
using Volo.Abp;
using Volo.Abp.Modularity;

namespace FclEx.Abp.Xunit;

[SuppressMessage("ReSharper", "ConvertToPrimaryConstructor")]
public abstract class AbstractAbpTests<TModule> where TModule : IAbpModule
{
    protected readonly ITestOutputHelper _output;
    protected readonly AbpTestsOptions _options = new();
    protected readonly Action<AbpTestsOptions>? _optionsBuilder;

    protected AbstractAbpTests(ITestOutputHelper output, Action<AbpTestsOptions>? optionsBuilder = null)
    {
        _output = output;
        _optionsBuilder = optionsBuilder;
    }

    protected IServiceProvider InitializeApp()
    {
        var watch = ValueStopwatch.StartNew();
        var config = BuildConfig();
        var services = _options.Services
            .AddSingleton<IConfigurationRoot>(config)
            .AddSingleton<IConfiguration>(config)
            .AddAbp<TModule>(Options)
            .AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel);
                builder.AddXunitTest(_output, true);
                builder.AddFilter("Volo.Abp.Modularity.ModuleManager", LogLevel.Warning);
                builder.AddFilter("Volo.Abp.AbpApplicationBase", LogLevel.Warning);
            });

        var provider = _options.UseAbpAsync
            ? SynchronizationContextScope.Run(services.UseAbpAsync)
            : services.UseAbp();

        var logger = provider.CreateLogger("FclEx.Abp.Xunit");
        logger.LogDebug("It takes {ElapsedSeconds} seconds to initialize abp framework", watch.GetElapsedTime().TotalSeconds);

        return provider;

        void Options(AbpApplicationCreationOptions creationOptions)
        {
            _optionsBuilder?.Invoke(_options);
            if (_options.UseLightInject)
            {
                creationOptions.UseLightInject(o => o.UseAop = _options.UseAop);
            }
        }
    }

    protected virtual LogLevel LogLevel => LogLevel.Trace;

    protected virtual IConfigurationRoot BuildConfig()
    {
        return new ConfigurationBuilder().Build();
    }
}