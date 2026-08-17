namespace FclEx.Caching;

public static class CacheExtensions
{
    public static bool TryGet<T>(this ICache<T> cache, string key, [NotNullWhen(true)] out T? item)
    {
        item = default;
        var (success, value, _, _) = Operation.Execute(() => cache.Get(key));
        if (!success) return false;
        item = value!.Value;
        return value.HasValue;
    }

    public static async Task<Optional<T>> TryGetAsync<T>(this ICache<T> cache, string key, CancellationToken cancellationToken = default)
    {
        var (success, value, _, _) = await Operation.ExecuteAsync(t => cache.GetAsync(key, t), cancellationToken: cancellationToken);
        return success
            ? Optional.Some(value!.Value)
            : Optional.None<T>();
    }

    public static bool TrySet<T>(this ICache<T> cache, string key, T item, TimeSpan? expiration = null)
    {
        var result = Operation.Execute(() => cache.Set(key, item, expiration));
        return result.IsSuccess;
    }

    public static async Task<bool> TrySetAsync<T>(this ICache<T> cache, string key, T item, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var result = await Operation.ExecuteAsync(t => cache.SetAsync(key, item, expiration, t), cancellationToken: cancellationToken);
        return result.IsSuccess;
    }
}