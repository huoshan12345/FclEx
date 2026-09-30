namespace FclEx.Logging;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RemoveLogging(this IServiceCollection services)
    {
        services.RemoveAll<ILoggerFactory>();
        services.RemoveAll<ILoggerProvider>();
        services.RemoveAll<ILogger>();
        return services;
    }

    public static IServiceCollection AddCollecting(this IServiceCollection services)
    {
        services.TryAddSingleton<CollectingLoggerProvider>();
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<ILoggerProvider, CollectingLoggerProvider>(
                sp => sp.GetRequiredService<CollectingLoggerProvider>()));
        return services;
    }
}
