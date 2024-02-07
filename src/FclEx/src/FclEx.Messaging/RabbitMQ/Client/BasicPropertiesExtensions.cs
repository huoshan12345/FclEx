using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using FclEx;

namespace RabbitMQ.Client;

public static class Extensions
{
    [return: NotNullIfNotNull("defaultValue")]
    public static T? Get<T>(this IBasicProperties prop, string key, T? defaultValue = default)
    {
        Check.NotNull(prop);

        var obj = prop.Headers?.Get(key);
        return obj.CastTo<T>() ?? defaultValue;
    }

    public static T GetOrSet<T>(this IBasicProperties prop, string key, Func<string, T> func)
    {
        Check.NotNull(prop);
        Check.NotNull(key);
        Check.NotNull(func);

        prop.Headers ??= new Dictionary<string, object>();
        if (!prop.Headers.TryGetValue(key, out var result))
        {
            result = func(key);
            prop.Headers[key] = result;
        }
        return result!.CastTo<T>();
    }

    public static IBasicProperties Set<T>(this IBasicProperties prop, string key, T value)
    {
        if (prop == null) throw new ArgumentNullException(nameof(prop));
        if (key == null) throw new ArgumentNullException(nameof(key));
        prop.Headers ??= new Dictionary<string, object>();
        prop.Headers[key] = value;
        return prop;
    }

    public static bool Has(this IBasicProperties prop, string key)
    {
        if (prop == null) throw new ArgumentNullException(nameof(prop));
        if (key == null) throw new ArgumentNullException(nameof(key));
        return prop.Headers != null && prop.Headers.ContainsKey(key);
    }

    public static int IncreaseErrorTimes(this IBasicProperties prop)
    {
        if (prop == null) throw new ArgumentNullException(nameof(prop));
        prop.Headers ??= new Dictionary<string, object>();
        var value = prop.Get(FclExAbpRabbitMqConstants.HeaderOfErrorTimes, 0);
        value++;
        prop.Headers[FclExAbpRabbitMqConstants.HeaderOfErrorTimes] = value;
        return value;
    }

    public static int GetErrorTimes(this IBasicProperties prop)
    {
        return Get<int>(prop, FclExAbpRabbitMqConstants.HeaderOfErrorTimes);
    }

    public static int GetDelayMilli(this IBasicProperties prop)
    {
        return prop?.Headers?.Get(FclExAbpRabbitMqConstants.HeaderOfDelayMilli)?.CastTo<int>() ?? 0;
    }

    public static TimeSpan GetDelay(this IBasicProperties prop)
    {
        return TimeSpan.FromMilliseconds(prop.GetDelayMilli());
    }

    public static IBasicProperties SetDelayMilli(this IBasicProperties prop, long milliSeconds)
    {
        if (prop == null) throw new ArgumentNullException(nameof(prop));
        if (milliSeconds <= 0)
            prop.Headers?.Remove(FclExAbpRabbitMqConstants.HeaderOfDelayMilli);
        else
        {
            prop.Headers ??= new Dictionary<string, object>();
            prop.Headers[FclExAbpRabbitMqConstants.HeaderOfDelayMilli] = milliSeconds;
        }
        return prop;
    }

    public static IBasicProperties SetDelay(this IBasicProperties prop, TimeSpan timeSpan)
    {
        return prop.SetDelayMilli((long)timeSpan.TotalMilliseconds);
    }
}