using System;

namespace EasyCaching.Core;

public static class EasyCachingProviderExtensions
{
    public static bool TryGet<T>(this IEasyCachingProvider provider, string key, [NotNullWhen(true)] out T? item)
    {
        item = default;
        var (success, value, _, _) = Operation.Execute(() => provider.Get<T>(key));
        if (success == false)
            return false;

        item = value!.Value;
        return value.HasValue;
    }

    public static T GetOrAdd<T>(this IEasyCachingProvider provider, string key, Func<string, T> func, TimeSpan expiration)
    {
        if (provider.TryGet<T>(key, out var obj) == false)
        {
            obj = func(key);
            provider.Set<T>(key, obj, expiration);
        }
        return obj;
    }
}