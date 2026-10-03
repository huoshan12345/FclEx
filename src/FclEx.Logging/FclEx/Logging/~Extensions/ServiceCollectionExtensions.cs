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
}
