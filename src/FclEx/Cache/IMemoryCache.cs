using System;
using System.Collections.Generic;
using System.Text;

namespace FclEx.Cache
{
    public interface IMemoryCache<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IDisposable
    {
        TValue GetOrAdd(TKey key, Func<TKey, TValue> activator);
        TValue AddOrUpdate(TKey key, TValue value);
        bool TryAdd(TKey key, TValue value);
        int Capacity { get; }
        CacheStats Stats { get; }
        bool Remove(TKey key);
        bool TryGetValue(TKey key, out TValue value);
        ICollection<TKey> Keys { get; }
        int Count { get; }
        void Clear();
    }
}
