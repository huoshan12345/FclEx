namespace FclEx.Utils;

/// <summary>
/// A thread-safe bounded cache that evicts the least frequently used entry.
/// </summary>
/// <remarks>
/// Entries with the same frequency are ordered by recency, so the least recently used entry wins an LFU tie. To prevent
/// historical traffic from permanently dominating the policy, all frequencies are halved after a configurable number
/// of policy-affecting accesses. Normal reads and writes are O(1); a decay pass is O(n log n).
/// </remarks>
[DebuggerDisplay("Count = {Count}, Capacity = {Capacity}")]
public sealed class LfuCache<TKey, TValue> : IBoundedCache<TKey, TValue> where TKey : notnull
{
    /// <summary>The default number of policy-affecting accesses between frequency decay passes.</summary>
    public const int DefaultFrequencyDecayInterval = 10_000;

    private readonly object _sync = new();
    private readonly Dictionary<TKey, Entry> _entries;
    private readonly LinkedList<FrequencyBucket> _frequencyBuckets = [];
    private readonly Dictionary<TKey, Lazy<TValue>> _pendingCreations;
    private int _accessesSinceDecay;
    private long _accessSequence;

    /// <summary>Initializes a cache with the specified capacity and frequency decay interval.</summary>
    /// <param name="capacity">The maximum number of entries.</param>
    /// <param name="frequencyDecayInterval">
    /// The number of successful reads, insertions, and writes between passes that halve every entry's frequency.
    /// </param>
    /// <param name="comparer">The comparer used for keys.</param>
    public LfuCache(
        int capacity,
        int frequencyDecayInterval = DefaultFrequencyDecayInterval,
        IEqualityComparer<TKey>? comparer = null)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Capacity must be greater than zero.");
        if (frequencyDecayInterval <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(frequencyDecayInterval),
                frequencyDecayInterval,
                "The frequency decay interval must be greater than zero.");
        }

        Capacity = capacity;
        FrequencyDecayInterval = frequencyDecayInterval;
        comparer ??= EqualityComparer<TKey>.Default;
        _entries = new Dictionary<TKey, Entry>(comparer);
        _pendingCreations = new Dictionary<TKey, Lazy<TValue>>(comparer);
    }

    /// <summary>Initializes a cache with the specified capacity and key comparer.</summary>
    public LfuCache(int capacity, IEqualityComparer<TKey>? comparer)
        : this(capacity, DefaultFrequencyDecayInterval, comparer)
    {
    }

    /// <inheritdoc />
    public event EventHandler<CacheEntryRemovedEventArgs<TKey, TValue>>? EntryRemoved;

    /// <inheritdoc />
    public int Capacity { get; }

    /// <summary>Gets the number of policy-affecting accesses between frequency decay passes.</summary>
    public int FrequencyDecayInterval { get; }

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
    /// <remarks>Keys are ordered from highest priority to the next eviction candidate.</remarks>
    public IReadOnlyCollection<TKey> Keys
    {
        get
        {
            lock (_sync)
                return SnapshotInternal().Select(pair => pair.Key).ToArray();
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
            {
                Touch(existing);
                return existing.Value;
            }

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
                Touch(existing);
                result = existing.Value;
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
                var oldValue = existing.Value;
                existing.Value = value;
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
            if (_entries.TryGetValue(key, out var entry))
            {
                Touch(entry);
                value = entry.Value;
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
            if (!_entries.TryGetValue(key, out var entry))
                return false;

            RemoveInternal(entry);
            notification = new CacheEntryRemovedEventArgs<TKey, TValue>(
                entry.Key,
                entry.Value,
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
            notifications = SnapshotInternal()
                .Select(pair => new CacheEntryRemovedEventArgs<TKey, TValue>(
                    pair.Key,
                    pair.Value,
                    CacheEntryRemovalReason.Cleared))
                .ToArray();
            _entries.Clear();
            _frequencyBuckets.Clear();
            _accessesSinceDecay = 0;
            _accessSequence = 0;
            handlers = EntryRemoved;
        }

        CacheNotificationHelper.Notify(this, handlers, notifications);
    }

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        KeyValuePair<TKey, TValue>[] snapshot;
        lock (_sync)
            snapshot = SnapshotInternal();
        return ((IEnumerable<KeyValuePair<TKey, TValue>>)snapshot).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private CacheEntryRemovedEventArgs<TKey, TValue>? AddInternal(TKey key, TValue value)
    {
        CacheEntryRemovedEventArgs<TKey, TValue>? notification = null;
        if (_entries.Count == Capacity)
        {
            var bucket = _frequencyBuckets.First!;
            var evicted = bucket.Value.Entries.Last!.Value;
            RemoveInternal(evicted);
            notification = new CacheEntryRemovedEventArgs<TKey, TValue>(
                evicted.Key,
                evicted.Value,
                CacheEntryRemovalReason.Evicted);
        }

        var firstBucket = _frequencyBuckets.First;
        if (firstBucket is null || firstBucket.Value.Frequency != 1)
            firstBucket = _frequencyBuckets.AddFirst(new FrequencyBucket(1));

        var entry = new Entry(key, value, ++_accessSequence, firstBucket);
        entry.Node = firstBucket.Value.Entries.AddFirst(entry);
        _entries.Add(key, entry);
        RecordPolicyAccess();
        return notification;
    }

    private void Touch(Entry entry)
    {
        var oldBucket = entry.Bucket;
        if (oldBucket.Value.Frequency == long.MaxValue)
        {
            DecayFrequencies();
            oldBucket = entry.Bucket;
        }

        var newFrequency = oldBucket.Value.Frequency + 1;
        var newBucket = oldBucket.Next;
        if (newBucket is null || newBucket.Value.Frequency != newFrequency)
            newBucket = _frequencyBuckets.AddAfter(oldBucket, new FrequencyBucket(newFrequency));

        oldBucket.Value.Entries.Remove(entry.Node!);
        entry.Bucket = newBucket;
        entry.Node = newBucket.Value.Entries.AddFirst(entry);
        entry.LastAccessSequence = ++_accessSequence;

        if (oldBucket.Value.Entries.Count == 0)
            _frequencyBuckets.Remove(oldBucket);

        RecordPolicyAccess();
    }

    private void RecordPolicyAccess()
    {
        _accessesSinceDecay++;
        if (_accessesSinceDecay >= FrequencyDecayInterval)
            DecayFrequencies();
    }

    private void DecayFrequencies()
    {
        _accessesSinceDecay = 0;
        if (_entries.Count == 0)
            return;

        var groups = _entries.Values
            .Select(entry => new
            {
                Entry = entry,
                Frequency = entry.Bucket.Value.Frequency / 2 + entry.Bucket.Value.Frequency % 2,
            })
            .OrderBy(item => item.Frequency)
            .ThenByDescending(item => item.Entry.LastAccessSequence)
            .GroupBy(item => item.Frequency)
            .ToArray();

        _frequencyBuckets.Clear();
        foreach (var group in groups)
        {
            var bucket = _frequencyBuckets.AddLast(new FrequencyBucket(group.Key));
            foreach (var item in group)
            {
                item.Entry.Bucket = bucket;
                item.Entry.Node = bucket.Value.Entries.AddLast(item.Entry);
            }
        }
    }

    private void RemoveInternal(Entry entry)
    {
        _entries.Remove(entry.Key);
        var bucket = entry.Bucket;
        bucket.Value.Entries.Remove(entry.Node!);
        if (bucket.Value.Entries.Count == 0)
            _frequencyBuckets.Remove(bucket);
    }

    private KeyValuePair<TKey, TValue>[] SnapshotInternal()
    {
        return _frequencyBuckets
            .Reverse()
            .SelectMany(bucket => bucket.Entries)
            .Select(entry => KeyValuePair.Create(entry.Key, entry.Value))
            .ToArray();
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

    private sealed class Entry
    {
        public Entry(TKey key, TValue value, long lastAccessSequence, LinkedListNode<FrequencyBucket> bucket)
        {
            Key = key;
            Value = value;
            LastAccessSequence = lastAccessSequence;
            Bucket = bucket;
        }

        public TKey Key { get; }
        public TValue Value { get; set; }
        public long LastAccessSequence { get; set; }
        public LinkedListNode<FrequencyBucket> Bucket { get; set; }
        public LinkedListNode<Entry>? Node { get; set; }
    }

    private sealed class FrequencyBucket
    {
        public FrequencyBucket(long frequency)
        {
            Frequency = frequency;
        }

        public long Frequency { get; }
        public LinkedList<Entry> Entries { get; } = [];
    }
}
