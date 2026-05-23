namespace FclEx.Utils;

[DebuggerDisplay("Count = {" + nameof(Count) + "}")]
public class LruCache<TKey, TValue> : IMemoryCache<TKey, TValue> where TKey : notnull
{
    private readonly LinkedList<KeyValue> _list = [];
    private readonly IDictionary<TKey, LinkedListNode<KeyValue>> _dic;
    private readonly ReaderWriterLockSlim _lock;
    private static readonly IEqualityComparer<TValue?> _valueComparer = EqualityComparer<TValue?>.Default;
    private readonly IEqualityComparer<TKey> _keyComparer;

    public LruCache(int? capacity = null, IEqualityComparer<TKey>? comparer = null)
    {
        if (capacity is <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity));

        Capacity = capacity ?? ushort.MaxValue;
        _keyComparer = comparer ?? EqualityComparer<TKey>.Default;
        _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        _dic = new Dictionary<TKey, LinkedListNode<KeyValue>>(_keyComparer);
    }

    public LruCache(IEqualityComparer<TKey>? comparer) : this(ushort.MaxValue, comparer)
    {
    }

    public TValue GetOrAdd(TKey key, Func<TKey, TValue> activator)
    {
        Check.NotNull(key);
        Check.NotNull(activator);

        using var _ = _lock.LockUpgradeableRead();

        var exist = _dic.TryGetValue(key, out var node);
        if (exist)
        {
            Debug.Assert(_keyComparer.Equals(key, node!.Value.Key));
        }

        using (_lock.LockWrite())
        {
            node = exist
                ? UpdateInternal(node!)
                : AddInternal(key, activator(key));
        }
        return node.Value.Value;
    }

    public TValue AddOrUpdate(TKey key, TValue value)
    {
        Check.NotNull(key);

        using var _ = _lock.LockUpgradeableRead();

        var exist = _dic.TryGetValue(key, out var node);
        if (exist)
        {
            Debug.Assert(_keyComparer.Equals(key, node!.Value.Key));
        }

        if (!(exist && _valueComparer.Equals(node!.Value.Value!, value!)))
        {
            using (_lock.LockWrite())
            {
                node = exist
                    ? UpdateInternal(node!, value)
                    : AddInternal(key, value);
            }
        }
        return node.Value.Value;
    }

    public bool TryAdd(TKey key, TValue value)
    {
        Check.NotNull(key);

        using var _ = _lock.LockUpgradeableRead();

        var exist = _dic.TryGetValue(key, out var node);
        if (exist)
        {
            Debug.Assert(_keyComparer.Equals(key, node!.Value.Key));
        }
        else
        {
            using (_lock.LockWrite())
            {
                node = AddInternal(key, value);
            }
        }
        return !exist;
    }

    public void Clear()
    {
        using (_lock.LockWrite())
        {
            _list.Clear();
            _dic.Clear();
        }
    }

    public bool Remove(TKey key)
    {
        return TryRemove(key, false, default);
    }

    public bool TryGetValue(TKey key, [NotNullWhen(true)] out TValue? value)
    {
        Check.NotNull(key);

        using var _ = _lock.LockRead();

        if (_dic.TryGetValue(key, out var node))
        {
            UpdateInternal(node);
            value = node.Value.Value!;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    public TValue this[TKey key]
    {
        get
        {
            if (!TryGetValue(key, out var value))
                throw new KeyNotFoundException($"The given key {key} was not present.");
            return value;
        }
        set => AddOrUpdate(key, value);
    }

    public int Count => Read(() => _list.Count);
    public int Capacity { get; }
    public ICollection<TKey> Keys => Read(() => _list.Select(m => m.Key).AsReadOnlyCollection());
    public ICollection<TValue> Values => Read(() => _list.Select(m => m.Value).AsReadOnlyCollection());

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        => LockEnumerator.Create(_list.Select(m => KeyValuePair.Create(m.Key, m.Value)).GetEnumerator(), _lock);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _lock.Dispose();
    }

    private T Read<T>(Func<T> func)
    {
        using (_lock.LockRead())
        {
            return func();
        }
    }

    private LinkedListNode<KeyValue> UpdateInternal(LinkedListNode<KeyValue> node)
    {
        var first = _list.First;
        if (node != first)
        {
            _list.Remove(node);
            _list.AddFirst(node);
        }
        return node;
    }

    private LinkedListNode<KeyValue> UpdateInternal(LinkedListNode<KeyValue> node, TValue value)
    {
        node.Value = node.Value with { Value = value };
        return UpdateInternal(node);
    }

    private LinkedListNode<KeyValue> AddInternal(TKey key, TValue value)
    {
        if (_dic.Count >= Capacity)
        {
            var toRemove = _list.Last;
            _dic.Remove(toRemove!.Value.Key);
            _list.Remove(toRemove);
        }
        var node = LinkedListNodeHelper.Create(new KeyValue(key, value));
        _list.AddFirst(node);
        _dic[key] = node;
        return node;
    }

    private bool TryRemove(TKey key, bool matchValue, TValue? oldValue)
    {
        Check.NotNull(key);

        using var _ = _lock.LockUpgradeableRead();

        var success = _dic.TryGetValue(key, out var node);
        if (success)
        {
            Debug.Assert(_keyComparer.Equals(key, node!.Value.Key));

            if (matchValue && !_valueComparer.Equals(oldValue, node.Value.Value))
                return false;

            using (_lock.LockWrite())
            {
                _list.Remove(node);
                _dic.Remove(key);
            }
        }
        return success;
    }

    internal readonly record struct KeyValue(TKey Key, TValue Value)
    {
        public static implicit operator KeyValuePair<TKey, TValue>(KeyValue kv)
            => KeyValuePair.Create(kv.Key, kv.Value!);
    }
}