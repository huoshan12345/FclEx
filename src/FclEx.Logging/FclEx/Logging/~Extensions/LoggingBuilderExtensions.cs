namespace FclEx.Logging;

/// <summary>Provides registration helpers for Microsoft logging builders.</summary>
public static class LoggingBuilderExtensions
{
    /// <summary>Registers a singleton collector alongside the builder's other logging providers.</summary>
    /// <param name="builder">The logging builder to configure.</param>
    /// <returns>The same builder for further configuration.</returns>
    /// <remarks>
    /// Repeated calls do not add duplicate collector registrations. Resolve
    /// <see cref="CollectingLoggerProvider"/> from dependency injection to inspect or clear collected entries.
    /// The concrete provider and its <see cref="ILoggerProvider"/> registration refer to the same instance.
    /// Factory filters still apply to logged messages.
    /// </remarks>
    public static ILoggingBuilder AddCollecting(this ILoggingBuilder builder)
    {
        builder.Services.TryAddSingleton<CollectingLoggerProvider>();
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<ILoggerProvider, CollectingLoggerProvider>(
                sp => sp.GetRequiredService<CollectingLoggerProvider>()));
        return builder;
    }
}
