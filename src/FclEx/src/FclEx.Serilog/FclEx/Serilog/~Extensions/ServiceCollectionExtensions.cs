using FclEx.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FclEx.Serilog;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RemoveLogging(this IServiceCollection services)
    {
        services.RemoveAll<ILoggerFactory>();
        services.RemoveAll<ILoggerProvider>();
        services.RemoveAll<global::Serilog.ILogger>();
        services.RemoveAll<Microsoft.Extensions.Logging.ILogger>();
        return services;
    }

    public static IServiceCollection AddSerilog<T>(this IServiceCollection services, Action<SerilogOptions>? configure = null) where T : LogProvider
    {
        var options = new SerilogOptions();
        configure?.Invoke(options);
        return services.AddSerilog<T>(options);
    }

    public static IServiceCollection AddSerilog(this IServiceCollection services, Action<SerilogOptions>? configure = null)
    {
        return services.AddSerilog<LogProvider>(configure);
    }

    public static IServiceCollection AddSerilog<T>(this IServiceCollection services, SerilogOptions? options) where T : LogProvider
    {
        options ??= new();

        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        var name = assembly.GetName().Name ?? string.Empty;

        services.RemoveLogging();
        // we filter logs by level with serilog instead of ms-log.
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Trace));
        services.AddSingleton(options);
        services.AddSingleton<T>();
        services.AddSingletonBy<global::Serilog.ILogger, T>(m =>
        {
            var logger = m.CreateSerilogLogger().ForContext(name);
            Log.Logger = logger;
            return logger;
        });
        services.AddSingletonBy<ILoggerProvider, global::Serilog.ILogger>(m => new SerilogLoggerProvider(m));
        services.AddSingletonBy<Microsoft.Extensions.Logging.ILogger, ILoggerFactory>(m => m.CreateLogger(name));

        return services;
    }

    public static IServiceCollection AddSerilog(this IServiceCollection services, SerilogOptions? options)
    {
        return services.AddSerilog<LogProvider>(options);
    }
}