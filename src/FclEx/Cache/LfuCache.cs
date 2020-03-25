using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using FclEx.Helpers;
using FclEx.Utils;
using MoreLinq;

namespace FclEx.Cache
{
    /// <summary>
    /// A very simple memory-cache which has the capacity.
    /// <para>If it is full before add new item, the minimum usage item will be removed.</para>
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public sealed class LfuCache<TKey, TValue> : IMemoryCache<TKey, TValue>, IDictionary<TKey, TValue>
    {
        private readonly LinkedList<KvCount> _list;
        private readonly IDictionary<TKey, LinkedListNode<KvCount>> _dic;
        private readonly ReaderWriterLockSlim _lock;
        private static readonly IEqualityComparer<TValue> _valueComparer = EqualityComparer<TValue>.Default;
        private readonly IEqualityComparer<TKey> _keyComparer;

        public LfuCache(int? capacity = null, IEqualityComparer<TKey> comparer = null)
        {
            if (capacity.HasValue && capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            Capacity = capacity ?? ushort.MaxValue;
            _keyComparer = comparer ?? EqualityComparer<TKey>.Default;
            _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            _dic = new Dictionary<TKey, LinkedListNode<KvCount>>(comparer);
            _list = new LinkedList<KvCount>();
            Stats = new CacheStats();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            using var _ = _lock.LockRead();
            if (_dic.TryGetValue(key, out var node))
            {
                node = UpdateInternal(node);
                Stats.OnHit();
                value = node.Value.Value;
                return true;
            }
            else
            {
                Stats.OnMiss();
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
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (activator == null) throw new ArgumentNullException(nameof(activator));

            using var _ = _lock.LockUpgradeableRead();

            var exist = _dic.TryGetValue(key, out var node);
            if (exist)
            {
                Debug.Assert(node != null);
                Debug.Assert(key.Equals(node.Value.Key));
                Stats.OnHit();
            }
            else
            {
                Stats.OnMiss();
            }
            using (_lock.LockWrite())
            {
                node = exist
                    ? UpdateInternal(node)
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
                Debug.Assert(node != null);
                Debug.Assert(key.Equals(node.Value.Key));
                Stats.OnHit();
            }
            else
            {
                Stats.OnMiss();
            }

            if (!(exist && _valueComparer.Equals(node.Value.Value, value)))
            {
                using (_lock.LockWrite())
                {
                    node = exist
                        ? UpdateInternal(node, value)
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
                Debug.Assert(node != null);
                Debug.Assert(key.Equals(node.Value.Key));
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

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return TryRemove(item.Key, true, item.Value);
        }

        public int Count => Read(() => _dic.Count);
        public bool IsReadOnly { get; } = false;
        public int Capacity { get; }

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

        public CacheStats Stats { get; }

        public ICollection<TKey> Keys => Read(() => _list.Select(m => m.Key).AsReadOnly());
        public ICollection<TValue> Values => Read(() => _list.Select(m => m.Value).AsReadOnly());

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => new SafeEnumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Dispose()
        {
            _lock?.Dispose();
        }

        private LinkedListNode<KvCount> UpdateInternal(LinkedListNode<KvCount> node, TValue value)
        {
            node.Value = node.Value.SetValue(value);
            return UpdateInternal(node);
        }

        private LinkedListNode<KvCount> UpdateInternal(LinkedListNode<KvCount> node)
        {
            Debug.Assert(node != null);
            var count = (node.Value = node.Value.Incre()).Count;
            var cur = node;
            while (cur.Previous != null)
            {
                var pCount = cur.Previous.Value.Count;
                if (count <= pCount)
                    break;
                cur = cur.Previous;
            }
            if (cur != node)
            {
                var tmp = cur.Value;
                cur.Value = node.Value;
                node.Value = tmp;
                _dic[cur.Value.Key] = cur;
                _dic[node.Value.Key] = node;
                node = cur;
            }
            return node;
        }

        private LinkedListNode<KvCount> AddInternal(TKey key, TValue value)
        {
            if (_dic.Count >= Capacity)
            {
                var toRemove = _list.Last;
                _dic.Remove(toRemove.Value.Key);
                _list.Remove(toRemove);
            }
            var node = LinkedListNodeHelper.Create(KvCount.Create(key, value));
            _list.AddLast(node);
            _dic[key] = node;
            return node;
        }

        private bool TryRemove(TKey key, bool matchValue, TValue oldValue)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            using var _ = _lock.LockUpgradeableRead();

            var success = _dic.TryGetValue(key, out var node);
            if (success)
            {
                Debug.Assert(node != null);
                Debug.Assert(_keyComparer.Equals(key, node.Value.Key));

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

        [DebuggerDisplay("({Key}, {Value}), {Count}")]
        internal readonly struct KvCount
        {
            private KvCount(TKey key, TValue value, int count = 0)
            {
                Key = key;
                Value = value;
                Count = count;
            }
            public TKey Key { get; }
            public TValue Value { get; }
            public int Count { get; }
            public static KvCount Create(TKey key, TValue value) => new KvCount(key, value);
            public KvCount Incre() => new KvCount(Key, Value, Count + 1);
            public KvCount SetValue(TValue value) => new KvCount(Key, value, Count);
            public static implicit operator KeyValuePair<TKey, TValue>(KvCount kv) => KvPair.Create(kv.Key, kv.Value);
        }

        internal readonly struct SafeEnumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private readonly ReaderWriterLockSlim _lock;
            private readonly IEnumerator<KvCount> _inner;

            public SafeEnumerator(LfuCache<TKey, TValue> cache)
            {
                _lock = cache._lock;
                _inner = cache._list.GetEnumerator();
                _lock.EnterReadLock();
            }

            public bool MoveNext()
            {
                return _inner.MoveNext();
            }

            public void Reset()
            {
                _inner.Reset();
            }

            public KeyValuePair<TKey, TValue> Current => _inner.Current;

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                _lock.ExitReadLock();
            }
        }

        private T Read<T>(Func<T> func)
        {
            using (_lock.LockRead())
            {
                return func();
            }
        }
    }
}
