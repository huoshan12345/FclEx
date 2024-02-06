using System;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Modularity;
using Xunit.Abstractions;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace FclEx.Abp.Xunit;

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

    protected IServiceProvider InitApp()
    {
        var watch = ValueStopwatch.StartNew();
        var config = BuildConfig();
        var services = _options.Services
            .AddSingleton<IConfigurationRoot>(config)
            .AddSingleton<IConfiguration>(config)
            .AddAbp<TModule>(Options)
            .AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddXunitTest(_output, false);
                builder.AddDebug();
                builder.AddFilter("Volo.Abp.Modularity.ModuleManager", LogLevel.Warning);
                builder.AddFilter("Volo.Abp.AbpApplicationBase", LogLevel.Warning);
            });

        var provider = services.UseAbp();

        var logger = provider.CreateLogger("FclEx.Abp.Xunit");
        logger.LogTrace($"It takes {watch.GetElapsedTime().TotalSeconds:f3} seconds to initialize abp framework");

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

    protected virtual IConfigurationRoot BuildConfig()
    {
        return new ConfigurationBuilder().Build();
    }
}