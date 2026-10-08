namespace FclEx.Logging;

/// <summary>Creates provider loggers using Microsoft's standard type category names.</summary>
public static class LoggerProviderExtensions
{
    /// <summary>Creates a logger with the same category as Microsoft's factory extension for the supplied type.</summary>
    /// <param name="provider">The provider to create the logger from.</param>
    /// <param name="type">The category type; generic arguments are excluded and nested types use dot separators.</param>
    /// <returns>The logger created by the provider.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="provider"/> or <paramref name="type"/> is null.</exception>
    public static ILogger CreateLogger(this ILoggerProvider provider, Type type)
    {
        Check.NotNull(provider);
        Check.NotNull(type);
        return new ProviderLoggerFactory(provider).CreateLogger(type);
    }

    /// <summary>Creates an untyped logger using Microsoft's standard category for <typeparamref name="T"/>.</summary>
    /// <typeparam name="T">The type whose logging category is used.</typeparam>
    /// <param name="provider">The provider to create the logger from.</param>
    /// <returns>The logger created by the provider.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="provider"/> is null.</exception>
    public static ILogger CreateLogger<T>(this ILoggerProvider provider)
    {
        return provider.CreateLogger(typeof(T));
    }

    // Delegate category formatting to Microsoft's extension without taking ownership of the provider.
    private sealed class ProviderLoggerFactory(ILoggerProvider provider) : ILoggerFactory
    {
        public ILogger CreateLogger(string categoryName) => provider.CreateLogger(categoryName);

        public void AddProvider(ILoggerProvider loggerProvider) => throw new NotSupportedException();

        public void Dispose() { }
    }
}
