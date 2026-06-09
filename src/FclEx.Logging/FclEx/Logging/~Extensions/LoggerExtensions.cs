using static FclEx.Logging.LogPropertyNames;

namespace FclEx.Logging;

public static class Extensions
{
    public static bool IsNullLogger(this ILogger logger)
    {
        var type = logger.GetType();

        return type == typeof(NullLogger)
               || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(NullLogger<>);
    }

    public static bool IsNullOrNullLogger([NotNullWhen(false)] this ILogger? logger)
    {
        return logger == null || logger.IsNullLogger();
    }

    public static ILogger With(this ILogger logger, IEnumerable<KeyValuePair<string, object?>> properties)
    {
        return new PropertiesLogger(logger, properties.Select(m => new LoggerProperty(m.Key, m.Value)));
    }

    public static ILogger With(this ILogger logger, params KeyValuePair<string, object?>[] properties)
    {
        return logger.With(properties.AsEnumerable());
    }

    public static ILogger With(this ILogger logger, IEnumerable<(string, object?)> properties)
    {
        return logger.With(properties.Select(m => KeyValuePair.Create(m.Item1, m.Item2)));
    }

    public static ILogger With(this ILogger logger, params (string, object?)[] properties)
    {
        return logger.With(properties.AsEnumerable());
    }

    public static ILogger With(this ILogger logger, string key, object? value)
    {
        return logger.With(KeyValuePair.Create(key, value));
    }

    public static ILogger With(this ILogger logger, (string key, object? value) property)
    {
        return logger.With(property.key, property.value);
    }

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public static IDisposable PushProperty(this ILogger logger, IEnumerable<KeyValuePair<string, object?>> properties)
    {
        return properties.IsNullOrEmpty()
            ? Disposable.Empty
            : logger.BeginScope(properties) ?? Disposable.Empty;
    }

    public static IDisposable PushProperty<T>(this ILogger logger, IEnumerable<KeyValuePair<string, T?>> properties)
    {
        return logger.PushProperty(properties.EmptyIfNull().Select(m => KeyValuePair.Create(m.Key, (object?)m.Value)));
    }

    public static IDisposable PushProperty(this ILogger logger, params KeyValuePair<string, object?>[] properties)
    {
        return logger.PushProperty(properties.EmptyIfNull().AsEnumerable());
    }

    public static IDisposable PushProperty(this ILogger logger, IEnumerable<(string, object?)> properties)
    {
        return logger.PushProperty(properties.EmptyIfNull().AsKeyValue());
    }

    public static IDisposable PushProperty<T>(this ILogger logger, IEnumerable<(string, T?)> properties)
    {
        return logger.PushProperty(properties.EmptyIfNull().Select(m => (m.Item1, (object?)m.Item2)));
    }

    public static IDisposable PushProperty(this ILogger logger, params (string, object?)[] properties)
    {
        return logger.PushProperty(properties.EmptyIfNull().AsEnumerable());
    }

    public static IDisposable PushProperty(this ILogger logger, string key, object? value)
    {
        return logger.PushProperty(KeyValuePair.Create(key, value));
    }

    public static IDisposable PushProperty(this ILogger logger, (string key, object? value) property)
    {
        return logger.PushProperty(property.key, property.value);
    }

    public static IDisposable PushProperty(this ILogger logger, LoggerProperty property)
    {
        return logger.PushProperty(property.Key, property.Value);
    }

    public static IDisposable PushProperty(this ILogger logger, IEnumerable<LoggerProperty> properties)
    {
        return logger.PushProperty(properties.Select(m => KeyValuePair.Create(m.Key, m.Value)));
    }

    public static LoggerProperties Properties(this ILogger logger)
    {
        return new LoggerProperties(logger);
    }

    public static LoggerProperties Properties(this ILogger logger, string name, object? value, bool destructureObjects = false)
    {
        return logger.Properties().Push(name, value, destructureObjects);
    }

    public static void LogOperation(this ILogger logger, string operationName, TimeSpan duration, LogLevel logLevel = LogLevel.Information)
    {
        logger.Log(logLevel, $"Execute {{{LogPropertyNames.Operation}}} successfully in {{{DurationSeconds}}}.", operationName, duration.ToSecondsString());
    }

    public static void LogOperationError(this ILogger logger, Exception ex, string operationName, TimeSpan duration, LogLevel logLevel = LogLevel.Error)
    {
        using var x = logger.Properties(DurationSeconds, duration.ToSecondsString());
        logger.LogError(ex, $"Failed to execute {{{LogPropertyNames.Operation}}} due to {{Error}}", operationName, ex.Message);
    }
}