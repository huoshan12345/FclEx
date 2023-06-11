using System.Collections.Generic;

namespace FclEx.Utils;

public interface IMemoryCache<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IDisposable where TKey : notnull
{
    TValue GetOrAdd(TKey key, Func<TKey, TValue> activator);
    TValue AddOrUpdate(TKey key, TValue value);
    bool TryAdd(TKey key, TValue value);
    TValue this[TKey key] { get; set; }
    int Capacity { get; }
    CacheStats Stats { get; }
    bool Remove(TKey key);
    bool TryGetValue(TKey key, [NotNullWhen(true)] out TValue? value);
    ICollection<TKey> Keys { get; }
    int Count { get; }
    void Clear();
}

public static class CacheExtensions
{
    public static bool IsFull<TKey, TValue>(this IMemoryCache<TKey, TValue> cache) where TKey : notnull
    {
        return cache.Count >= cache.Capacity;
    }
}