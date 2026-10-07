namespace FclEx.Logging;

/// <summary>Provides helpers for removing Microsoft logging service registrations.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Removes logger factories, providers, untyped loggers, and open or closed typed logger registrations.</summary>
    /// <param name="services">The service collection to modify before building a service provider.</param>
    /// <returns>The same collection for further configuration.</returns>
    /// <remarks>
    /// This unregisters logging rather than substituting null loggers. It does not dispose registered instances,
    /// change existing service providers, or remove options and separately registered concrete provider types.
    /// Logging can be registered again with <c>AddLogging</c>.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is null.</exception>
    public static IServiceCollection RemoveLogging(this IServiceCollection services)
    {
        Check.NotNull(services);
        for (var i = services.Count - 1; i >= 0; i--)
        {
            var type = services[i].ServiceType;
            if (type == typeof(ILoggerFactory) || type == typeof(ILoggerProvider) || type == typeof(ILogger)
                || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ILogger<>))
            {
                services.RemoveAt(i);
            }
        }
        return services;
    }
}
