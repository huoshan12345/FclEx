namespace FclEx.Caching.Redis;

public static class RedisCollectionExtensions
{
    public static Task<long> LPushAsync<T>(this IRedisList<T> col, T cacheValue) => col.LPushAsync([cacheValue]);
    public static Task<long> RPushAsync<T>(this IRedisList<T> col, T cacheValue) => col.RPushAsync([cacheValue]);
    public static Task<long> SAddAsync<T>(this IRedisSet<T> col, T cacheValue) => col.SAddAsync([cacheValue]);
    public static Task<long> SRemAsync<T>(this IRedisSet<T> col, T cacheValue) => col.SRemAsync([cacheValue]);
    public static Task<long> ZAddAsync<T>(this IRedisSortedSet<T> col, T cacheValue, double score) where T : notnull
        => col.ZAddAsync(new Dictionary<T, double> { [cacheValue] = score });
    public static Task<long> ZRemAsync<T>(this IRedisSortedSet<T> col, T cacheValue) where T : notnull
        => col.ZRemAsync([cacheValue]);
}