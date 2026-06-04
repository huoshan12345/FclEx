// ReSharper disable ConvertToAutoPropertyWhenPossible
namespace FclEx.Utils;

/// <summary>
/// A very simple memory-cache which has the capacity.
/// <para>If it is full before add new item, the minimum usage item will be removed.</para>
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
[DebuggerDisplay("Count = {" + nameof(Count) + "}")]
public sealed class LfuCache<TKey, TValue> : IMemoryCache<TKey, TValue> where TKey : notnull
{
    private static readonly IEqualityComparer<TValue> _valueComparer = EqualityComparer<TValue>.Default;
    private readonly LinkedList<KvCount> _list = [];
    private readonly IDictionary<TKey, LinkedListNode<KvCount>> _dic;
    private readonly ReaderWriterLockSlim _lock;
    private readonly IEqualityComparer<TKey> _keyComparer;
    private readonly int _capacity;

    public event Action<TKey, TValue> OnItemCleared = (key, value) => { };

    public LfuCache(int? capacity = null, IEqualityComparer<TKey>? comparer = null)
    {
        if (capacity is <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity));

        _capacity = capacity ?? int.MaxValue;
        _keyComparer = comparer ?? EqualityComparer<TKey>.Default;
        _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        _dic = new Dictionary<TKey, LinkedListNode<KvCount>>(_keyComparer);
    }

    public LfuCache(IEqualityComparer<TKey>? comparer) : this(ushort.MaxValue, comparer)
    {
    }

    public bool TryGetValue(TKey key, [NotNullWhen(true), MaybeNullWhen(false)] out TValue value)
    {
        Check.NotNull(key);

        using var _ = _lock.LockRead();
        if (_dic.TryGetValue(key, out var node))
        {
            node = UpdateInternal(node);
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

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        return TryRemove(item.Key, true, item.Value);
    }

    public int Count => Read(() => _dic.Count);
    public bool IsReadOnly { get; } = false;
    public int Capacity => _capacity;

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    public void Clear()
    {
        using (_lock.LockWrite())
        {
            _list.Clear();
            _dic.Clear();
        }
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        if (!TryGetValue(item.Key, out var value))
            return false;
        return _valueComparer.Equals(value, item.Value);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        if (array == null) throw new ArgumentNullException(nameof(array));
        if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex), arrayIndex, "Non-negative number required.");
        if (arrayIndex > array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex), arrayIndex, "Larger than collection size.");
        if (array.Length - arrayIndex < Count) throw new ArgumentException("The specified space is not sufficient to copy the elements from this Collection.");

        using (_lock.LockRead())
        {
            foreach (var item in _list)
            {
                array[arrayIndex++] = item;
            }
        }
    }

    public void Add(TKey key, TValue value)
    {
        if (!TryAdd(key, value))
            throw new ArgumentException($"The key {key} already existed.");
    }

    public bool ContainsKey(TKey key)
    {
        return TryGetValue(key, out _);
    }

    public bool Remove(TKey key)
    {
        return TryRemove(key, false, default);
    }

    public ICollection<TKey> Keys => Read(() => _list.Select(m => m.Key).AsReadOnlyCollection());

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        => LockEnumerator.Create(_list.Select(m => KeyValuePair.Create(m.Key, m.Value)).GetEnumerator(), _lock);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Dispose()
    {
        _lock.Dispose();
    }

    private LinkedListNode<KvCount> UpdateInternal(LinkedListNode<KvCount> node, TValue value)
    {
        node.Value = node.Value.SetValue(value);
        return UpdateInternal(node);
    }

    private LinkedListNode<KvCount> UpdateInternal(LinkedListNode<KvCount> node)
    {
        var count = (node.Value = node.Value.Increment()).Count;
        var cur = node;
        while (cur.Previous != null)
        {
            var pCount = cur.Previous.Value.Count;
            if (count <= pCount)
                break;
            cur = cur.Previous;
        }
        // ReSharper disable once InvertIf
        if (cur != node)
        {
            (cur.Value, node.Value) = (node.Value, cur.Value);
            _dic[cur.Value.Key] = cur;
            _dic[node.Value.Key] = node;
            node = cur;
        }
        return node;
    }

    private LinkedListNode<KvCount> AddInternal(TKey key, TValue value)
    {
        if (_dic.Count >= _capacity)
        {
            var toRemove = _list.Last!;
            _dic.Remove(toRemove.Value.Key);
            _list.Remove(toRemove);

            OnItemCleared(toRemove.Value.Key, toRemove.Value.Value);
        }
        var node = LinkedListNodeHelper.Create(new KvCount(key, value));
        _list.AddLast(node);
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

            if (matchValue && !_valueComparer.Equals(oldValue!, node.Value.Value))
                return false;

            using (_lock.LockWrite())
            {
                _list.Remove(node);
                _dic.Remove(key);
            }
        }
        return success;
    }

    internal readonly record struct KvCount(TKey Key, TValue Value, int Count = 0)
    {
        public KvCount Increment() => this with { Count = Count + 1 };
        public KvCount SetValue(TValue value) => this with { Value = value };
        public static implicit operator KeyValuePair<TKey, TValue>(KvCount kv) => KeyValuePair.Create(kv.Key, kv.Value!);
    }

    private T Read<T>(Func<T> func)
    {
        using (_lock.LockRead())
        {
            return func();
        }
    }
}