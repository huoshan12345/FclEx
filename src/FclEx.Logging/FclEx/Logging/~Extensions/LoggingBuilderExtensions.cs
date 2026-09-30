namespace FclEx.Logging;

public static class LoggingBuilderExtensions
{
    public static ILoggingBuilder AddCollecting(this ILoggingBuilder builder)
    {
        builder.Services.TryAddSingleton<CollectingLoggerProvider>();
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<ILoggerProvider, CollectingLoggerProvider>(
                sp => sp.GetRequiredService<CollectingLoggerProvider>()));
        return builder;
    }
}
