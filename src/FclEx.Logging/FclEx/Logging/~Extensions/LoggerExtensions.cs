using static FclEx.Logging.LogPropertyNames;

namespace FclEx.Logging;

/// <summary>Provides property scopes, property-enriched wrappers, null checks, and operation duration logging.</summary>
public static class LoggerExtensions
{
    /// <summary>Checks whether a logger is Microsoft's untyped or typed null logger.</summary>
    /// <param name="logger">The logger to inspect.</param>
    /// <returns>True only for a <see cref="NullLogger"/> or a <see cref="NullLogger{T}"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> is null.</exception>
    public static bool IsNullLogger(this ILogger logger)
    {
        Check.NotNull(logger);
        var type = logger.GetType();

        return type == typeof(NullLogger)
               || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(NullLogger<>);
    }

    /// <summary>Checks whether a logger is null or a Microsoft null logger instance.</summary>
    /// <param name="logger">The optional logger to inspect.</param>
    /// <returns>True for null or a Microsoft null logger; false for other logger implementations.</returns>
    public static bool IsNullOrNullLogger([NotNullWhen(false)] this ILogger? logger)
    {
        return logger == null || logger.IsNullLogger();
    }

    /// <summary>Creates a wrapper that applies fixed properties as temporary scopes to each enabled log call.</summary>
    /// <param name="logger">The logger receiving messages.</param>
    /// <param name="properties">Property definitions copied at construction; their values are not deep copied.</param>
    /// <returns>A wrapper that restores its temporary scopes after each log call, including failed calls.</returns>
    /// <remarks>Duplicate keys are preserved; the provider determines precedence and must support structured scopes.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> or <paramref name="properties"/> is null.</exception>
    public static ILogger With(this ILogger logger, IEnumerable<KeyValuePair<string, object?>> properties)
    {
        return new PropertiesLogger(logger, properties.Select(m => new LoggerProperty(m.Key, m.Value)));
    }

    /// <inheritdoc cref="With(ILogger, IEnumerable{KeyValuePair{string, object}})"/>
    public static ILogger With(this ILogger logger, params KeyValuePair<string, object?>[] properties)
    {
        return logger.With(properties.AsEnumerable());
    }

    /// <inheritdoc cref="With(ILogger, IEnumerable{KeyValuePair{string, object}})"/>
    public static ILogger With(this ILogger logger, IEnumerable<(string, object?)> properties)
    {
        return logger.With(properties.Select(m => KeyValuePair.Create(m.Item1, m.Item2)));
    }

    /// <inheritdoc cref="With(ILogger, IEnumerable{KeyValuePair{string, object}})"/>
    public static ILogger With(this ILogger logger, params (string, object?)[] properties)
    {
        return logger.With(properties.AsEnumerable());
    }

    /// <summary>Creates a wrapper that applies one fixed property to each enabled log call.</summary>
    /// <param name="logger">The logger receiving messages.</param>
    /// <param name="key">The structured property name.</param>
    /// <param name="value">The property value, retained by reference.</param>
    /// <returns>A property-enriched wrapper; the underlying logger's scopes are restored after each call.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> is null.</exception>
    public static ILogger With(this ILogger logger, string key, object? value)
    {
        return logger.With(KeyValuePair.Create(key, value));
    }

    /// <summary>Creates a wrapper that applies one name/value tuple to each enabled log call.</summary>
    /// <param name="logger">The logger receiving messages.</param>
    /// <param name="property">The structured property name and value.</param>
    /// <returns>A wrapper with a temporary property scope for each enabled log call.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> is null.</exception>
    public static ILogger With(this ILogger logger, (string key, object? value) property)
    {
        return logger.With(property.key, property.value);
    }

    /// <summary>Begins a scope containing a snapshot of structured property definitions.</summary>
    /// <param name="logger">The logger on which to begin the scope.</param>
    /// <param name="properties">Properties enumerated once before beginning the scope; null or empty starts no scope.</param>
    /// <returns>The scope handle, or an empty disposable for no properties or unsupported scopes.</returns>
    /// <remarks>
    /// Values are retained by reference. Dispose scopes in reverse creation order. Duplicate keys are
    /// preserved and the provider determines their precedence. Enumeration exceptions propagate before a scope begins.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> is null.</exception>
    public static IDisposable PushProperty(this ILogger logger, IEnumerable<KeyValuePair<string, object?>>? properties)
    {
        Check.NotNull(logger);
        var snapshot = properties?.ToArray();
        return snapshot is null || snapshot.Length == 0
            ? Disposable.Empty
            : logger.BeginScope(snapshot) ?? Disposable.Empty;
    }

    /// <summary>Begins a structured property scope, boxing values when necessary.</summary>
    /// <typeparam name="T">The property value type.</typeparam>
    /// <param name="logger">The logger on which to begin the scope.</param>
    /// <param name="properties">Properties enumerated once; null or empty starts no scope.</param>
    /// <returns>A scope handle to dispose in reverse creation order, or an empty disposable.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> is null.</exception>
    public static IDisposable PushProperty<T>(this ILogger logger, IEnumerable<KeyValuePair<string, T?>>? properties)
    {
        return logger.PushProperty(properties.EmptyIfNull().Select(m => KeyValuePair.Create(m.Key, (object?)m.Value)));
    }

    /// <inheritdoc cref="PushProperty(ILogger, IEnumerable{KeyValuePair{string, object}})"/>
    public static IDisposable PushProperty(this ILogger logger, params KeyValuePair<string, object?>[]? properties)
    {
        return logger.PushProperty(properties.EmptyIfNull().AsEnumerable());
    }

    /// <inheritdoc cref="PushProperty(ILogger, IEnumerable{KeyValuePair{string, object}})"/>
    public static IDisposable PushProperty(this ILogger logger, IEnumerable<(string, object?)>? properties)
    {
        return logger.PushProperty(properties.EmptyIfNull().AsKeyValue());
    }

    /// <summary>Begins a structured property scope from name/value tuples, boxing values when necessary.</summary>
    /// <typeparam name="T">The property value type.</typeparam>
    /// <param name="logger">The logger on which to begin the scope.</param>
    /// <param name="properties">Tuples enumerated once; null or empty starts no scope.</param>
    /// <returns>A scope handle to dispose in reverse creation order, or an empty disposable.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> is null.</exception>
    public static IDisposable PushProperty<T>(this ILogger logger, IEnumerable<(string, T?)>? properties)
    {
        return logger.PushProperty(properties.EmptyIfNull().Select(m => (m.Item1, (object?)m.Item2)));
    }

    /// <inheritdoc cref="PushProperty(ILogger, IEnumerable{KeyValuePair{string, object}})"/>
    public static IDisposable PushProperty(this ILogger logger, params (string, object?)[]? properties)
    {
        return logger.PushProperty(properties.EmptyIfNull().AsEnumerable());
    }

    /// <summary>Immediately begins a structured scope containing one property.</summary>
    /// <param name="logger">The logger on which to begin the scope.</param>
    /// <param name="key">The structured property name.</param>
    /// <param name="value">The property value, retained by reference.</param>
    /// <returns>A scope handle to dispose in reverse creation order, or an empty disposable for unsupported scopes.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> is null.</exception>
    public static IDisposable PushProperty(this ILogger logger, string key, object? value)
    {
        return logger.PushProperty(KeyValuePair.Create(key, value));
    }

    /// <summary>Immediately begins a structured scope containing one name/value tuple.</summary>
    /// <param name="logger">The logger on which to begin the scope.</param>
    /// <param name="property">The property name and value.</param>
    /// <returns>A scope handle to dispose in reverse creation order, or an empty disposable for unsupported scopes.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> is null.</exception>
    public static IDisposable PushProperty(this ILogger logger, (string key, object? value) property)
    {
        return logger.PushProperty(property.key, property.value);
    }

    /// <summary>Immediately begins a structured scope containing one property definition.</summary>
    /// <param name="logger">The logger on which to begin the scope.</param>
    /// <param name="property">The property name and value.</param>
    /// <returns>A scope handle to dispose in reverse creation order, or an empty disposable for unsupported scopes.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> is null.</exception>
    public static IDisposable PushProperty(this ILogger logger, LoggerProperty property)
    {
        return logger.PushProperty(property.Key, property.Value);
    }

    /// <inheritdoc cref="PushProperty(ILogger, IEnumerable{KeyValuePair{string, object}})"/>
    public static IDisposable PushProperty(this ILogger logger, IEnumerable<LoggerProperty>? properties)
    {
        return logger.PushProperty(properties.EmptyIfNull().Select(m => KeyValuePair.Create(m.Key, m.Value)));
    }

    /// <summary>Creates an empty group for property scopes that will be released together.</summary>
    /// <param name="logger">The logger on which subsequent pushes start scopes.</param>
    /// <returns>An empty scope group; dispose it to release pushed scopes in reverse order.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> is null.</exception>
    public static LoggerPropertyScope Properties(this ILogger logger)
    {
        return new LoggerPropertyScope(logger);
    }

    /// <summary>Creates a property scope group and immediately pushes its first property.</summary>
    /// <param name="logger">The logger on which to begin scopes.</param>
    /// <param name="name">The nonempty structured property name.</param>
    /// <param name="value">The property value, which may be null.</param>
    /// <returns>The scope group, which can accept more properties and releases its scopes when disposed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> or <paramref name="name"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="name"/> is empty.</exception>
    public static LoggerPropertyScope Properties(this ILogger logger, string name, object? value)
    {
        return new LoggerPropertyScope(logger, name, value);
    }

    /// <summary>Logs an operation's successful completion and supplied duration at the requested level.</summary>
    /// <param name="logger">The logger receiving the message.</param>
    /// <param name="operationName">The operation name recorded as a structured property.</param>
    /// <param name="duration">The measured duration, formatted as a seconds string; this method does not measure time.</param>
    /// <param name="logLevel">The level used for the message.</param>
    public static void LogOperation(this ILogger logger, string operationName, TimeSpan duration, LogLevel logLevel = LogLevel.Information)
    {
        logger.Log(logLevel, $"Execute {{{LogPropertyNames.Operation}}} successfully in {{{DurationSeconds}}}.", operationName, duration.ToSecondsString());
    }

    /// <summary>Logs an operation failure and exception at the requested level, with the supplied duration in a temporary scope.</summary>
    /// <param name="logger">The logger receiving the failure message.</param>
    /// <param name="ex">The failure exception; its message is also recorded as a structured error property.</param>
    /// <param name="operationName">The operation name recorded as a structured property.</param>
    /// <param name="duration">The measured duration, formatted as a seconds string; this method does not measure time.</param>
    /// <param name="logLevel">The level used for the message; defaults to Error.</param>
    public static void LogOperationError(this ILogger logger, Exception ex, string operationName, TimeSpan duration, LogLevel logLevel = LogLevel.Error)
    {
        using var x = logger.Properties(DurationSeconds, duration.ToSecondsString());
        logger.Log(logLevel, ex, $"Failed to execute {{{LogPropertyNames.Operation}}} due to {{Error}}", operationName, ex.Message);
    }
}
