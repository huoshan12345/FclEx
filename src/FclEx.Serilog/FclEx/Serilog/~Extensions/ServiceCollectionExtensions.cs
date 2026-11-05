using FclEx.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FclEx.Serilog;

public static class ServiceCollectionExtensions
{
    /// <summary>Registers a lazily created, container-owned Serilog logger as the Microsoft logging provider.</summary>
    /// <param name="services">The service collection whose logging registrations will be replaced.</param>
    /// <param name="configure">The action that configures each root logger, with common exclusions enabled.</param>
    /// <returns>The same service collection.</returns>
    /// <remarks>The action runs when a container first resolves logging services.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="configure"/> is null.</exception>
    public static IServiceCollection AddSerilog(this IServiceCollection services, Action<LoggerConfiguration> configure)
    {
        Check.NotNull(services);
        Check.NotNull(configure);
        return services.AddSerilog(new SerilogConfiguration().AddCommonExcluders().Configure(configure));
    }

    /// <summary>Registers a lazily created Serilog logger owned by each root service provider.</summary>
    /// <param name="services">The service collection whose logging registrations will be replaced.</param>
    /// <param name="options">The completed configuration, or null to use defaults with common exclusions.</param>
    /// <returns>The same service collection.</returns>
    /// <remarks>
    /// Microsoft logging builder actions run during registration. Each root provider creates its own
    /// Serilog logger on first resolution and disposes it during shutdown. Use asynchronous provider
    /// disposal on supported targets for asynchronous sink cleanup. Complete configuration before
    /// registration, and create per-container disposable sinks inside Serilog configuration actions.
    /// Assign <see cref="Log.Logger"/> explicitly in the application when global logging is needed.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is null.</exception>
    public static IServiceCollection AddSerilog(this IServiceCollection services, SerilogConfiguration? options = null)
    {
        Check.NotNull(services);
        options ??= new SerilogConfiguration().AddCommonExcluders();

        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        var name = assembly.GetName().Name ?? string.Empty;

        services.RemoveLogging();
        services.RemoveAll<global::Serilog.ILogger>();

        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Trace);
            options.LoggingBuilderConfigure?.Invoke(builder);
        });

        // Factory registration transfers ownership of each created root logger to its container.
        services.AddSingleton<global::Serilog.ILogger>(_ => options.CreateLogger());
        services.AddSingleton<ILoggerProvider>(provider => new SerilogLoggerProviderWithoutExternalScope(
            provider.GetRequiredService<global::Serilog.ILogger>(), dispose: false));
        services.AddSingletonBy<Microsoft.Extensions.Logging.ILogger, ILoggerFactory>(factory => factory.CreateLogger(name));

        return services;
    }
}
