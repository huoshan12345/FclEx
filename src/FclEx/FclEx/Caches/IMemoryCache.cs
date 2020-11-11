using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FclEx.Caches
{
    public interface IMemoryCache<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IDisposable
    {
        [return: MaybeNull] TValue GetOrAdd(TKey key, Func<TKey, TValue> activator);
        [return: MaybeNull] TValue AddOrUpdate(TKey key, [AllowNull] TValue value);
        bool TryAdd(TKey key, [AllowNull] TValue value);
        [MaybeNull] TValue this[TKey key] { get; set; }
        int Capacity { get; }
        CacheStats Stats { get; }
        bool Remove(TKey key);
        bool TryGetValue(TKey key, [NotNullWhen(true), MaybeNullWhen(false)] out TValue value);
        ICollection<TKey> Keys { get; }
        int Count { get; }
        void Clear();
    }
}
