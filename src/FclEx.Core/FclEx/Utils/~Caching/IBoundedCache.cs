namespace FclEx.Utils;

/// <summary>
/// Represents a thread-safe key/value cache that removes entries when its capacity is exceeded.
/// </summary>
/// <remarks>
/// Reading an entry may update its eviction priority. Enumeration and <see cref="Keys"/> return snapshots and do not
/// affect eviction priority.
/// </remarks>
public interface IBoundedCache<TKey, TValue> : IReadOnlyCollection<KeyValuePair<TKey, TValue>> where TKey : notnull
{
    /// <summary>
    /// Raised after an entry has been removed from the cache for any reason.
    /// </summary>
    /// <remarks>
    /// Handlers run after the cache has released its internal lock. If handlers throw, the cache operation has already
    /// completed; all registered handlers are still invoked and their exceptions are propagated afterwards.
    /// </remarks>
    event EventHandler<CacheEntryRemovedEventArgs<TKey, TValue>>? EntryRemoved;

    /// <summary>Gets the maximum number of entries held by the cache.</summary>
    int Capacity { get; }

    /// <summary>Gets a snapshot of the cached keys in eviction-policy order.</summary>
    IReadOnlyCollection<TKey> Keys { get; }

    /// <summary>
    /// Gets an existing value or creates and caches one.
    /// </summary>
    /// <remarks>
    /// Concurrent calls for the same missing key share one invocation of <paramref name="valueFactory"/>. Factories for
    /// different keys may run concurrently, and factories always run outside the cache lock.
    /// </remarks>
    TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory);

    /// <summary>Adds or replaces an entry and updates its eviction priority.</summary>
    void Set(TKey key, TValue value);

    /// <summary>Adds an entry if the key is absent without changing an existing entry's eviction priority.</summary>
    bool TryAdd(TKey key, TValue value);

    /// <summary>Tries to get an entry and updates its eviction priority when found.</summary>
    bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value);

    /// <summary>Removes an entry if it exists.</summary>
    bool Remove(TKey key);

    /// <summary>Removes every entry.</summary>
    void Clear();

    /// <summary>Gets or sets an entry. Getting and setting update its eviction priority.</summary>
    TValue this[TKey key] { get; set; }
}

/// <summary>Describes why a cache entry was removed.</summary>
public enum CacheEntryRemovalReason
{
    /// <summary>The entry was evicted to make room for another entry.</summary>
    Evicted,

    /// <summary>The entry was explicitly removed.</summary>
    Removed,

    /// <summary>The entry was replaced by another value.</summary>
    Replaced,

    /// <summary>The cache was cleared.</summary>
    Cleared,
}

/// <summary>Provides information about an entry removed from a bounded cache.</summary>
public sealed class CacheEntryRemovedEventArgs<TKey, TValue> : EventArgs where TKey : notnull
{
    /// <summary>Initializes a removal notification.</summary>
    public CacheEntryRemovedEventArgs(TKey key, TValue value, CacheEntryRemovalReason reason)
    {
        Key = key;
        Value = value;
        Reason = reason;
    }

    /// <summary>Gets the removed entry's key.</summary>
    public TKey Key { get; }

    /// <summary>Gets the removed entry's value.</summary>
    public TValue Value { get; }

    /// <summary>Gets the reason the entry was removed.</summary>
    public CacheEntryRemovalReason Reason { get; }
}

public static class BoundedCacheExtensions
{
    /// <summary>Returns whether the cache currently contains its maximum number of entries.</summary>
    public static bool IsFull<TKey, TValue>(this IBoundedCache<TKey, TValue> cache) where TKey : notnull
    {
        Check.NotNull(cache);
        return cache.Count >= cache.Capacity;
    }
}
