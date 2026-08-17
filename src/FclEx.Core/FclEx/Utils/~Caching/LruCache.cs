namespace FclEx.Utils;

/// <summary>
/// A thread-safe bounded cache that evicts the least recently used entry.
/// </summary>
/// <remarks>
/// Successful reads and writes make an entry the most recently used. Snapshot operations such as enumeration and
/// <see cref="Keys"/> do not affect recency.
/// </remarks>
[DebuggerDisplay("Count = {Count}, Capacity = {Capacity}")]
public sealed class LruCache<TKey, TValue> : IBoundedCache<TKey, TValue> where TKey : notnull
{
    private readonly object _sync = new();
    private readonly Dictionary<TKey, LinkedListNode<Entry>> _entries;
    private readonly LinkedList<Entry> _recency = [];
    private readonly Dictionary<TKey, Lazy<TValue>> _pendingCreations;

    /// <summary>Initializes a cache with the specified maximum number of entries.</summary>
    public LruCache(int capacity, IEqualityComparer<TKey>? comparer = null)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Capacity must be greater than zero.");

        Capacity = capacity;
        comparer ??= EqualityComparer<TKey>.Default;
        _entries = new Dictionary<TKey, LinkedListNode<Entry>>(comparer);
        _pendingCreations = new Dictionary<TKey, Lazy<TValue>>(comparer);
    }

    /// <inheritdoc />
    public event EventHandler<CacheEntryRemovedEventArgs<TKey, TValue>>? EntryRemoved;

    /// <inheritdoc />
    public int Capacity { get; }

    /// <inheritdoc />
    public int Count
    {
        get
        {
            lock (_sync)
                return _entries.Count;
        }
    }

    /// <inheritdoc />
    public IReadOnlyCollection<TKey> Keys
    {
        get
        {
            lock (_sync)
                return _recency.Select(entry => entry.Key).ToArray();
        }
    }

    /// <inheritdoc />
    public TValue this[TKey key]
    {
        get => TryGetValue(key, out var value)
            ? value
            : throw new KeyNotFoundException($"The key '{key}' was not found in the cache.");
        set => Set(key, value);
    }

    /// <inheritdoc />
    public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
    {
        Check.NotNull(key);
        Check.NotNull(valueFactory);

        Lazy<TValue> creation;
        lock (_sync)
        {
            if (_entries.TryGetValue(key, out var existing))
                return Touch(existing).Value.Value;

            if (!_pendingCreations.TryGetValue(key, out creation!))
            {
                creation = new Lazy<TValue>(
                    () => valueFactory(key),
                    LazyThreadSafetyMode.ExecutionAndPublication);
                _pendingCreations.Add(key, creation);
            }
        }

        TValue createdValue;
        try
        {
            createdValue = creation.Value;
        }
        catch
        {
            RemovePendingCreation(key, creation);
            throw;
        }

        CacheEntryRemovedEventArgs<TKey, TValue>? notification = null;
        TValue result;
        EventHandler<CacheEntryRemovedEventArgs<TKey, TValue>>? handlers;
        lock (_sync)
        {
            RemovePendingCreationInternal(key, creation);
            if (_entries.TryGetValue(key, out var existing))
            {
                result = Touch(existing).Value.Value;
            }
            else
            {
                notification = AddInternal(key, createdValue);
                result = createdValue;
            }
            handlers = EntryRemoved;
        }

        Notify(handlers, notification);
        return result;
    }

    /// <inheritdoc />
    public void Set(TKey key, TValue value)
    {
        Check.NotNull(key);

        CacheEntryRemovedEventArgs<TKey, TValue>? notification;
        EventHandler<CacheEntryRemovedEventArgs<TKey, TValue>>? handlers;
        lock (_sync)
        {
            if (_entries.TryGetValue(key, out var existing))
            {
                var oldValue = existing.Value.Value;
                existing.Value = new Entry(key, value);
                Touch(existing);
                notification = ReferenceEquals(oldValue, value)
                    ? null
                    : new CacheEntryRemovedEventArgs<TKey, TValue>(key, oldValue, CacheEntryRemovalReason.Replaced);
            }
            else
            {
                notification = AddInternal(key, value);
            }
            handlers = EntryRemoved;
        }

        Notify(handlers, notification);
    }

    /// <inheritdoc />
    public bool TryAdd(TKey key, TValue value)
    {
        Check.NotNull(key);

        CacheEntryRemovedEventArgs<TKey, TValue>? notification;
        EventHandler<CacheEntryRemovedEventArgs<TKey, TValue>>? handlers;
        lock (_sync)
        {
            if (_entries.ContainsKey(key))
                return false;

            notification = AddInternal(key, value);
            handlers = EntryRemoved;
        }

        Notify(handlers, notification);
        return true;
    }

    /// <inheritdoc />
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        Check.NotNull(key);

        lock (_sync)
        {
            if (_entries.TryGetValue(key, out var node))
            {
                value = Touch(node).Value.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    /// <inheritdoc />
    public bool Remove(TKey key)
    {
        Check.NotNull(key);

        CacheEntryRemovedEventArgs<TKey, TValue>? notification;
        EventHandler<CacheEntryRemovedEventArgs<TKey, TValue>>? handlers;
        lock (_sync)
        {
            if (!_entries.TryGetValue(key, out var node))
                return false;

            _entries.Remove(key);
            _recency.Remove(node);
            notification = new CacheEntryRemovedEventArgs<TKey, TValue>(
                node.Value.Key,
                node.Value.Value,
                CacheEntryRemovalReason.Removed);
            handlers = EntryRemoved;
        }

        Notify(handlers, notification);
        return true;
    }

    /// <inheritdoc />
    public void Clear()
    {
        CacheEntryRemovedEventArgs<TKey, TValue>[] notifications;
        EventHandler<CacheEntryRemovedEventArgs<TKey, TValue>>? handlers;
        lock (_sync)
        {
            notifications = _recency
                .Select(entry => new CacheEntryRemovedEventArgs<TKey, TValue>(
                    entry.Key,
                    entry.Value,
                    CacheEntryRemovalReason.Cleared))
                .ToArray();
            _entries.Clear();
            _recency.Clear();
            handlers = EntryRemoved;
        }

        CacheNotificationHelper.Notify(this, handlers, notifications);
    }

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        KeyValuePair<TKey, TValue>[] snapshot;
        lock (_sync)
        {
            snapshot = _recency
                .Select(entry => KeyValuePair.Create(entry.Key, entry.Value))
                .ToArray();
        }
        return ((IEnumerable<KeyValuePair<TKey, TValue>>)snapshot).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private LinkedListNode<Entry> Touch(LinkedListNode<Entry> node)
    {
        if (node != _recency.First)
        {
            _recency.Remove(node);
            _recency.AddFirst(node);
        }
        return node;
    }

    private CacheEntryRemovedEventArgs<TKey, TValue>? AddInternal(TKey key, TValue value)
    {
        CacheEntryRemovedEventArgs<TKey, TValue>? notification = null;
        if (_entries.Count == Capacity)
        {
            var evicted = _recency.Last!;
            _recency.RemoveLast();
            _entries.Remove(evicted.Value.Key);
            notification = new CacheEntryRemovedEventArgs<TKey, TValue>(
                evicted.Value.Key,
                evicted.Value.Value,
                CacheEntryRemovalReason.Evicted);
        }

        var node = _recency.AddFirst(new Entry(key, value));
        _entries.Add(key, node);
        return notification;
    }

    private void RemovePendingCreation(TKey key, Lazy<TValue> creation)
    {
        lock (_sync)
            RemovePendingCreationInternal(key, creation);
    }

    private void RemovePendingCreationInternal(TKey key, Lazy<TValue> creation)
    {
        if (_pendingCreations.TryGetValue(key, out var current) && ReferenceEquals(current, creation))
            _pendingCreations.Remove(key);
    }

    private void Notify(
        EventHandler<CacheEntryRemovedEventArgs<TKey, TValue>>? handlers,
        CacheEntryRemovedEventArgs<TKey, TValue>? notification)
    {
        if (notification is not null)
            CacheNotificationHelper.Notify(this, handlers, new[] { notification });
    }

    private readonly record struct Entry(TKey Key, TValue Value);
}
