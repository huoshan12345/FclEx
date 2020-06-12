using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace FclEx.Cache
{
    public interface IMemoryCache<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IDisposable
    {
        [return: MaybeNull] TValue GetOrAdd(TKey key, Func<TKey, TValue> activator);
        [return: MaybeNull] TValue AddOrUpdate(TKey key, TValue value);
        bool TryAdd(TKey key, [AllowNull] TValue value);
        int Capacity { get; }
        CacheStats Stats { get; }
        bool Remove(TKey key);
        bool TryGetValue(TKey key, [MaybeNull] out TValue value);
        ICollection<TKey> Keys { get; }
        int Count { get; }
        void Clear();
    }
}
