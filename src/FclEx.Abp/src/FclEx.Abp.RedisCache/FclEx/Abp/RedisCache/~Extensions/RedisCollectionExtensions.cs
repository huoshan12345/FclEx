namespace FclEx.Abp.RedisCache;

public static class RedisCollectionExtensions
{
    public static long LPush<T>(this IRedisList<T> col, T cacheValue) => col.LPush([cacheValue]);
    public static long RPush<T>(this IRedisList<T> col, T cacheValue) => col.RPush([cacheValue]);
    public static Task<long> LPushAsync<T>(this IRedisList<T> col, T cacheValue) => col.LPushAsync([cacheValue]);
    public static Task<long> RPushAsync<T>(this IRedisList<T> col, T cacheValue) => col.RPushAsync([cacheValue]);

    public static long SAdd<T>(this IRedisSet<T> col, T cacheValue) => col.SAdd([cacheValue]);
    public static long SRem<T>(this IRedisSet<T> col, T cacheValue) => col.SRem([cacheValue]);
    public static Task<long> SAddAsync<T>(this IRedisSet<T> col, T cacheValue) => col.SAddAsync([cacheValue]);
    public static Task<long> SRemAsync<T>(this IRedisSet<T> col, T cacheValue) => col.SRemAsync([cacheValue]);

    public static long ZAdd<T>(this IRedisSortedSet<T> col, T cacheValue, double score) where T : notnull
        => col.ZAdd(new Dictionary<T, double> { [cacheValue] = score });

    public static long ZRem<T>(this IRedisSortedSet<T> col, T cacheValue) where T : notnull
        => col.ZRem([cacheValue]);

    public static Task<long> ZAddAsync<T>(this IRedisSortedSet<T> col, T cacheValue, double score) where T : notnull
        => col.ZAddAsync(new Dictionary<T, double> { [cacheValue] = score });

    public static Task<long> ZRemAsync<T>(this IRedisSortedSet<T> col, T cacheValue) where T : notnull
        => col.ZRemAsync([cacheValue]);
}