namespace FclEx.Logging;

public static class LoggerFactoryExtensions
{
    public static ILoggerFactory DefaultIfNull(this ILoggerFactory? factory)
    {
        return factory ?? NullLoggerFactory.Instance;
    }

    /// <summary>Creates a logger for a type, using a null logger factory when no factory is supplied.</summary>
    /// <param name="factory">The logger factory, or null to disable logging.</param>
    /// <param name="type">The type whose standard logging category is used.</param>
    /// <returns>A logger for the type, or a null logger when <paramref name="factory"/> is null.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is null.</exception>
    public static ILogger CreateLoggerOrDefault(this ILoggerFactory? factory, Type type)
    {
        return factory.DefaultIfNull().CreateLogger(type);
    }

    /// <summary>Creates a typed logger, using a null logger factory when no factory is supplied.</summary>
    /// <typeparam name="T">The type whose logging category is used.</typeparam>
    /// <param name="factory">The logger factory, or null to disable logging.</param>
    /// <returns>A typed logger that delegates to the supplied factory, or discards messages when it is null.</returns>
    public static ILogger<T> CreateLoggerOrDefault<T>(this ILoggerFactory? factory)
    {
        return factory.DefaultIfNull().CreateLogger<T>();
    }

    /// <summary>Adds a new collector to a logger factory and returns it for inspection.</summary>
    /// <param name="factory">The factory to which the provider is added.</param>
    /// <returns>The newly added provider, whose entries include messages allowed by the factory's filters.</returns>
    /// <remarks>
    /// Each call adds an independent collector. With Microsoft's <see cref="LoggerFactory"/>,
    /// the factory disposes the added provider when it is disposed; collected entries remain readable.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="factory"/> is null.</exception>
    public static CollectingLoggerProvider AddCollecting(this ILoggerFactory factory)
    {
        Check.NotNull(factory);

        var provider = new CollectingLoggerProvider();
        factory.AddProvider(provider);
        return provider;
    }
}
