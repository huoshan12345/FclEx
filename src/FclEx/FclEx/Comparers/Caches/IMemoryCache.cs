using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FclEx.Caches
{
    public interface IMemoryCache<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IDisposable where TKey : notnull
    {
        TValue? GetOrAdd(TKey key, Func<TKey, TValue> activator);
        TValue? AddOrUpdate(TKey key, [AllowNull] TValue value);
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
