using Volo.Abp.Modularity;

namespace FclEx.Abp;

public class AbpTestsFixture<TModule> : CoreTestsFixture where TModule : AbpModule
{
    private readonly Lazy<IServiceProvider> _services;
    public IServiceProvider Services => _services.Value;

    protected virtual LogLevel LogLevel => LogLevel.Trace;

    public AbpTestsFixture()
    {
        _services = new(Build);
    }

    protected virtual IServiceCollection CreateServices()
    {
        var config = BuildConfig();
        var services = new ServiceCollection()
            .AddSingleton(config)
            .AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.SetMinimumLevel(LogLevel);
                builder.AddXunit();
            })
            .AddAbp<TModule>();

        return services;
    }

    protected virtual IServiceProvider Build()
    {
        var services = CreateServices();
        return services.BuildServiceProviderFromFactory();
    }

    public override async ValueTask InitializeAsync()
    {
        await Services.UseAbpAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        if (!_services.IsValueCreated)
            return;

        await Services.CloseAbpAsync();

        Disposable.FromValue(Services).Dispose();
    }
}

public class AbpTestsFixture : AbpTestsFixture<AbpTestsModule>;
