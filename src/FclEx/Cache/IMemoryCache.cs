using System;
using System.Collections.Generic;
using System.Text;

namespace FclEx.Cache
{
    public interface IMemoryCache<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IDisposable
    {
        bool TryGet(TKey key, out TValue value);
        TValue GetOrAdd(TKey key, Func<TKey, TValue> activator);
        TValue AddOrUpdate(TKey key, TValue value);
        bool TryAdd(TKey key, TValue value);
        int Count { get; }
        int Capacity { get; }
        void Clear();
        bool Remove(TKey key);
        IReadOnlyList<TKey> GetKeys();
        CacheStats Stats { get; }
    }
}
