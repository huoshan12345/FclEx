using System.Collections.Generic;
using System.Threading.Tasks;
using FclEx.Abp.RedisCache.Collections;

namespace FclEx.Abp.RedisCache
{
    public static class RedisColExtensions
    {
        public static long LPush<T>(this IRedisList<T> col, T cacheValue) => col.LPush(new[] { cacheValue });
        public static long RPush<T>(this IRedisList<T> col, T cacheValue) => col.RPush(new[] { cacheValue });
        public static Task<long> LPushAsync<T>(this IRedisList<T> col, T cacheValue) => col.LPushAsync(new[] { cacheValue });
        public static Task<long> RPushAsync<T>(this IRedisList<T> col, T cacheValue) => col.RPushAsync(new[] { cacheValue });

        public static long SAdd<T>(this IRedisSet<T> col, T cacheValue) => col.SAdd(new[] { cacheValue });
        public static long SRem<T>(this IRedisSet<T> col, T cacheValue) => col.SRem(new[] { cacheValue });
        public static Task<long> SAddAsync<T>(this IRedisSet<T> col, T cacheValue) => col.SAddAsync(new[] { cacheValue });
        public static Task<long> SRemAsync<T>(this IRedisSet<T> col, T cacheValue) => col.SRemAsync(new[] { cacheValue });

        public static long ZAdd<T>(this IRedisSortedSet<T> col, T cacheValue, double score) where T : notnull
            => col.ZAdd(new Dictionary<T, double> { [cacheValue] = score });

        public static long ZRem<T>(this IRedisSortedSet<T> col, T cacheValue) where T : notnull
            => col.ZRem(new[] { cacheValue });

        public static Task<long> ZAddAsync<T>(this IRedisSortedSet<T> col, T cacheValue, double score) where T : notnull
            => col.ZAddAsync(new Dictionary<T, double> { [cacheValue] = score });

        public static Task<long> ZRemAsync<T>(this IRedisSortedSet<T> col, T cacheValue) where T : notnull
            => col.ZRemAsync(new[] { cacheValue });
    }
}
