using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using FclEx.Helpers;

namespace FclEx.Utils;

[DebuggerDisplay("Count = {" + nameof(Count) + "}")]
public class LruCache<TKey, TValue> : IMemoryCache<TKey, TValue> where TKey : notnull
{
    private readonly LinkedList<KeyValue> _list;
    private readonly IDictionary<TKey, LinkedListNode<KeyValue>> _dic;
    private readonly ReaderWriterLockSlim _lock;
    private static readonly IEqualityComparer<TValue?> _valueComparer = EqualityComparer<TValue?>.Default;
    private readonly IEqualityComparer<TKey> _keyComparer;

    public LruCache(int? capacity = null, IEqualityComparer<TKey>? comparer = null)
    {
        if (capacity is <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
        Capacity = capacity ?? ushort.MaxValue;
        _keyComparer = comparer ?? EqualityComparer<TKey>.Default;
        _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        _dic = new Dictionary<TKey, LinkedListNode<KeyValue>>(_keyComparer);
        _list = new LinkedList<KeyValue>();
        Stats = new CacheStats();
    }

    public TValue GetOrAdd(TKey key, Func<TKey, TValue> activator)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        if (activator == null) throw new ArgumentNullException(nameof(activator));

        using var _ = _lock.LockUpgradeableRead();

        var exist = _dic.TryGetValue(key, out var node);
        if (exist)
        {
            Debug.Assert(key.Equals(node!.Value.Key));
            Stats.OnHit();
        }
        else
        {
            Stats.OnMiss();
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
        if (key == null) throw new ArgumentNullException(nameof(key));

        using var _ = _lock.LockUpgradeableRead();

        var exist = _dic.TryGetValue(key, out var node);
        if (exist)
        {
            Debug.Assert(key.Equals(node!.Value.Key));
            Stats.OnHit();
        }
        else
        {
            Stats.OnMiss();
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
        if (key == null) throw new ArgumentNullException(nameof(key));

        using var _ = _lock.LockUpgradeableRead();

        var exist = _dic.TryGetValue(key, out var node);
        if (exist)
        {
            Debug.Assert(key.Equals(node!.Value.Key));
            Stats.OnHit();
        }
        else
        {
            Stats.OnMiss();

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
        if (key == null) throw new ArgumentNullException(nameof(key));

        using var _ = _lock.LockRead();
        if (_dic.TryGetValue(key, out var node))
        {
            UpdateInternal(node);
            Stats.OnHit();
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
    public ICollection<TKey> Keys => Read(() => _list.Select(m => m.Key).AsReadOnly());
    public ICollection<TValue> Values => Read(() => _list.Select(m => m.Value).AsReadOnly())!;
    public CacheStats Stats { get; }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        => LockEnumerator.Create(_list.Select(m => KvPair.Create(m.Key, m.Value)).GetEnumerator(), _lock);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Dispose()
    {
        _lock?.Dispose();
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
        node.Value = node.Value.SetValue(value);
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
        var node = LinkedListNodeHelper.Create(KeyValue.Create(key, value));
        _list.AddFirst(node);
        _dic[key] = node;
        return node;
    }

    private bool TryRemove(TKey key, bool matchValue, TValue? oldValue)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));

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

    [DebuggerDisplay("({Key}, {Value})")]
    internal readonly struct KeyValue
    {
        private KeyValue(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
        public TKey Key { get; }
        public TValue Value { get; }
        public static KeyValue Create(TKey key, TValue value) => new(key, value);
        public KeyValue SetValue(TValue value) => new(Key, value);
        public static implicit operator KeyValuePair<TKey, TValue>(KeyValue kv) => KvPair.Create(kv.Key, kv.Value!);
    }
}