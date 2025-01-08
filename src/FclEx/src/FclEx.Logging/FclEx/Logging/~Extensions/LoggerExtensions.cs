namespace FclEx.Logging;

public static class Extensions
{
    public static bool IsNullOrNullLogger([NotNullWhen(false)] this ILogger? logger)
    {
        if (logger == null) return true;

        var type = logger.GetType();

        return type == typeof(NullLogger)
               || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(NullLogger<>);
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
}