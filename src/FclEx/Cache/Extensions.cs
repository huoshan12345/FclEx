namespace FclEx.Cache
{
    public static class Extensions
    {
        public static bool IsFull<TKey, TValue>(this IMemoryCache<TKey, TValue> cache)
        {
            return cache.Count >= cache.Capacity;
        }

        public static bool Contains<TKey, TValue>(this IMemoryCache<TKey, TValue> cache, TKey key)
        {
            return cache.TryGet(key, out _);
        }
    }
}
