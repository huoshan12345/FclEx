using FclEx.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FclEx.Serilog;

public static class ServiceCollectionExtensions
{
    /// <summary>Configures and registers a Serilog logger as the Microsoft logging provider.</summary>
    /// <param name="services">The service collection whose logging registrations will be replaced.</param>
    /// <param name="configure">The action that configures the Serilog logger, with common exclusions enabled.</param>
    /// <returns>The same service collection.</returns>
    /// <remarks>The created logger is also assigned to <see cref="Log.Logger"/>.</remarks>
    public static IServiceCollection AddSerilog(this IServiceCollection services, Action<LoggerConfiguration> configure)
    {
        return services.AddSerilog((m, _) => configure(m));
    }

    /// <summary>Configures Serilog and its Microsoft logging integration before registering the logger.</summary>
    /// <param name="services">The service collection whose logging registrations will be replaced.</param>
    /// <param name="configure">The action that customizes the Serilog logger and its integration options.</param>
    /// <returns>The same service collection.</returns>
    /// <remarks>
    /// Common exclusions are enabled by default. Logging builder actions registered by the callback run
    /// after the Serilog configuration actions complete. The created logger is also assigned to <see cref="Log.Logger"/>.
    /// </remarks>
    public static IServiceCollection AddSerilog(this IServiceCollection services, Action<LoggerConfiguration, SerilogConfiguration> configure)
    {
        var options = new SerilogConfiguration().AddCommonExcluders();
        options.Configure((m, n) => configure(m, n));
        return services.AddSerilog(options);
    }

    /// <summary>Creates a Serilog logger and registers it as the Microsoft logging provider.</summary>
    /// <param name="services">The service collection whose logging registrations will be replaced.</param>
    /// <param name="options">The Serilog configuration, or null to use defaults with common exclusions.</param>
    /// <returns>The same service collection.</returns>
    /// <remarks>
    /// Serilog configuration actions complete before Microsoft logging builder actions run.
    /// The created logger is also assigned to <see cref="Log.Logger"/>.
    /// </remarks>
    public static IServiceCollection AddSerilog(this IServiceCollection services, SerilogConfiguration? options = null)
    {
        options ??= new SerilogConfiguration().AddCommonExcluders();

        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        var name = assembly.GetName().Name ?? string.Empty;

        services.RemoveLogging();
        services.RemoveAll<global::Serilog.ILogger>();

        var logger = options.CreateLogger();
        try
        {
            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                options.LoggingBuilderConfigure?.Invoke(builder);
            });
        }
        catch
        {
            (logger as IDisposable)?.Dispose();
            throw;
        }

        Log.Logger = logger;

        services.AddSingleton(logger);
        services.AddSingleton<ILoggerProvider>(new SerilogLoggerProviderWithoutExternalScope(logger));
        services.AddSingletonBy<Microsoft.Extensions.Logging.ILogger, ILoggerFactory>(m => m.CreateLogger(name));

        return services;
    }
}
