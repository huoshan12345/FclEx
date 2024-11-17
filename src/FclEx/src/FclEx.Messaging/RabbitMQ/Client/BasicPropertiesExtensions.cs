namespace RabbitMQ.Client;

public static class Extensions
{
    public static BasicProperties AsBasicProperties(this IReadOnlyBasicProperties properties)
    {
        return properties as BasicProperties ?? new BasicProperties(properties);
    }

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static T? Get<T>(this IBasicProperties properties, string key, T? defaultValue = default)
    {
        Check.NotNull(properties);

        var obj = properties.Headers?.Get(key);
        return obj.CastTo<T>() ?? defaultValue;
    }

    public static T GetOrSet<T>(this IBasicProperties properties, string key, Func<string, T> func)
    {
        Check.NotNull(properties);
        Check.NotNull(key);
        Check.NotNull(func);

        properties.Headers ??= new Dictionary<string, object?>();
        if (properties.Headers.TryGetValue(key, out var result) == false)
        {
            result = func(key);
            properties.Headers[key] = result;
        }
        return result!.CastTo<T>();
    }

    public static IBasicProperties Set<T>(this IBasicProperties properties, string key, T value)
    {
        Check.NotNull(properties);
        Check.NotNull(key);

        properties.Headers ??= new Dictionary<string, object?>();
        properties.Headers[key] = value;
        return properties;
    }

    public static bool Has(this IBasicProperties properties, string key)
    {
        Check.NotNull(properties);
        Check.NotNull(key);

        return properties.Headers != null && properties.Headers.ContainsKey(key);
    }

    public static int IncreaseErrorTimes(this IBasicProperties properties)
    {
        Check.NotNull(properties);

        properties.Headers ??= new Dictionary<string, object?>();
        var value = properties.Get(RabbitMQHeaderNames.ErrorTimes, 0);
        value++;
        properties.Headers[RabbitMQHeaderNames.ErrorTimes] = value;
        return value;
    }

    public static int GetErrorTimes(this IBasicProperties properties)
    {
        return Get<int>(properties, RabbitMQHeaderNames.DelayMilli);
    }

    public static int GetDelayMilli(this IBasicProperties properties)
    {
        return properties.Headers?.Get(RabbitMQHeaderNames.DelayMilli)?.CastTo<int>() ?? 0;
    }

    public static TimeSpan GetDelay(this IBasicProperties properties)
    {
        return TimeSpan.FromMilliseconds(properties.GetDelayMilli());
    }

    public static IBasicProperties SetDelayMilli(this IBasicProperties properties, long milliSeconds)
    {
        Check.NotNull(properties);

        if (milliSeconds <= 0)
        {
            properties.Headers?.Remove(RabbitMQHeaderNames.DelayMilli);
        }
        else
        {
            properties.Headers ??= new Dictionary<string, object?>();
            properties.Headers[RabbitMQHeaderNames.DelayMilli] = milliSeconds;
        }
        return properties;
    }

    public static IBasicProperties SetDelay(this IBasicProperties properties, TimeSpan timeSpan)
    {
        return properties.SetDelayMilli((long)timeSpan.TotalMilliseconds);
    }
}