using System.Reflection;

namespace RabbitMQ.Client;

public static class Extensions
{
    public static BasicProperties AsBasicProperties(this IReadOnlyBasicProperties properties)
    {
        return properties as BasicProperties ?? new BasicProperties(properties);
    }

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static T? Get<T>(this IReadOnlyBasicProperties properties, string key, T? defaultValue = default)
    {
        Check.NotNull(properties);

        var obj = properties.Headers?.Get(key);
        return obj.CastTo<T>() ?? defaultValue;
    }

    public static IReadOnlyBasicProperties Set<T>(this IReadOnlyBasicProperties properties, string key, T value)
    {
        Check.NotNull(properties);
        Check.NotNull(key);

        var headers = properties.GetOrCreateHeaders();
        headers[key] = value;
        return properties;
    }

    public static T Upsert<T>(this IReadOnlyBasicProperties properties, string key, Func<T, T> func, T defaultValue)
    {
        Check.NotNull(properties);
        Check.NotNull(key);
        Check.NotNull(func);

        var headers = properties.GetOrCreateHeaders();
        var result = defaultValue;
        if (headers.TryGetValue(key, out var obj) && obj is not null)
        {
            var value = obj.CastTo<T>();
            result = func(value);
        }
        headers[key] = result;
        return result;
    }

    public static bool Has(this IReadOnlyBasicProperties properties, string key)
    {
        Check.NotNull(properties);
        Check.NotNull(key);

        return properties.Headers != null && properties.Headers.ContainsKey(key);
    }

    public static int IncreaseErrorTimes(this IReadOnlyBasicProperties properties)
    {
        return properties.Upsert(RabbitMQHeaderNames.ErrorTimes, m => m + 1, 1);
    }

    public static int GetErrorTimes(this IReadOnlyBasicProperties properties)
    {
        return Get<int>(properties, RabbitMQHeaderNames.ErrorTimes);
    }

    public static int GetDelayMilli(this IReadOnlyBasicProperties properties)
    {
        return properties.Headers?.Get(RabbitMQHeaderNames.DelayMilli)?.CastTo<int>() ?? 0;
    }

    public static TimeSpan GetDelay(this IReadOnlyBasicProperties properties)
    {
        return TimeSpan.FromMilliseconds(properties.GetDelayMilli());
    }

    public static IReadOnlyBasicProperties SetDelayMilli(this IReadOnlyBasicProperties properties, long milliSeconds)
    {
        Check.NotNull(properties);

        if (milliSeconds <= 0)
        {
            properties.Headers?.Remove(RabbitMQHeaderNames.DelayMilli);
        }
        else
        {
            var headers = properties.GetOrCreateHeaders();
            headers[RabbitMQHeaderNames.DelayMilli] = milliSeconds;
        }
        return properties;
    }

    public static IReadOnlyBasicProperties SetDelay(this IReadOnlyBasicProperties properties, TimeSpan timeSpan)
    {
        return properties.SetDelayMilli((long)timeSpan.TotalMilliseconds);
    }

    private static readonly FieldInfo _headers = typeof(ReadOnlyBasicProperties).GetRequiredField("_headers");

    public static IDictionary<string, object?> GetOrCreateHeaders(this IReadOnlyBasicProperties properties)
    {
        Check.NotNull(properties);

        if (properties is IBasicProperties basic)
        {
            basic.Headers ??= new Dictionary<string, object?>();
            return basic.Headers;
        }

        if (properties is ReadOnlyBasicProperties readOnly)
        {
            var headers = _headers.GetValue<Dictionary<string, object?>>(readOnly);
            if (headers is null)
            {
                headers = new Dictionary<string, object?>();
                _headers.SetValue(readOnly, headers);
            }
            return headers;
        }

        throw new NotSupportedException("Not supported properties type: " + properties.GetType());
    }
}