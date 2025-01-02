using FclEx.DependencyInjection;
using FclEx.Logging;
using Microsoft.Extensions.Logging;

namespace FclEx.Serilog;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSerilog(this IServiceCollection services, Action<LoggerConfiguration> configure)
    {
        return services.AddSerilog((m, _) => configure(m));
    }

    public static IServiceCollection AddSerilog(this IServiceCollection services, Action<LoggerConfiguration, SerilogConfiguration> configure)
    {
        var options = new SerilogConfiguration().AddCommonExcluders();
        options.Configure((m, n) => configure(m, n));
        return services.AddSerilog(options);
    }

    public static IServiceCollection AddSerilog(this IServiceCollection services, SerilogConfiguration? options = null)
    {
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

        var logger = options.CreateSerilogLogger();
        Log.Logger = logger;

        services.AddSingleton(logger);
        services.AddSingleton<ILoggerProvider>(new SerilogLoggerProvider(logger));
        services.AddSingletonBy<Microsoft.Extensions.Logging.ILogger, ILoggerFactory>(m => m.CreateLogger(name));

        return services;
    }
}