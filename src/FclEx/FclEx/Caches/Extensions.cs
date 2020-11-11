namespace FclEx.Caches
{
    public static class Extensions
    {
        public static bool IsFull<TKey, TValue>(this IMemoryCache<TKey, TValue> cache)
        {
            return cache.Count >= cache.Capacity;
        }
    }
}
