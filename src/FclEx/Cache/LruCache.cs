using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using FclEx.Utils;

namespace FclEx.Cache
{
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public class LruCache<TKey, TValue> : IMemoryCache<TKey, TValue>, IDictionary<TKey, TValue>
    {
        private readonly LinkedList<KeyValuePair<TKey, TValue>> _list;
        private readonly IDictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>> _dic;
        private readonly ReaderWriterLockSlim _lock;
        private static readonly IEqualityComparer<TValue> _valueComparer = EqualityComparer<TValue>.Default;
        private readonly IEqualityComparer<TKey> _keyComparer;

        public LruCache(int? capacity = null, IEqualityComparer<TKey> comparer = null)
        {
            if (capacity.HasValue && capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            Capacity = capacity ?? ushort.MaxValue;
            _keyComparer = comparer ?? EqualityComparer<TKey>.Default;
            _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            _dic = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>(_keyComparer);
            _list = new LinkedList<KeyValuePair<TKey, TValue>>();
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

        private LinkedListNode<KeyValuePair<TKey, TValue>> UpdateInternal(LinkedListNode<KeyValuePair<TKey, TValue>> node)
        {
            Debug.Assert(node != null);
            var first = _list.First;
            if (node != first)
            {
                _list.Remove(node);
                _list.AddFirst(node);
            }
            return node;
        }

        private LinkedListNode<KeyValuePair<TKey, TValue>> UpdateInternal(LinkedListNode<KeyValuePair<TKey, TValue>> node, TValue value)
        {
            node.Value = node.Value.SetValue(value);
            return UpdateInternal(node);
        }

        private LinkedListNode<KeyValuePair<TKey, TValue>> AddInternal(TKey key, TValue value)
        {
            if (_dic.Count >= Capacity)
            {
                var toRemove = _list.Last;
                _dic.Remove(toRemove.Value.Key);
                _list.Remove(toRemove);
            }
            var node = LinkedListNodeHelper.Create(KvPair.For(key, value));
            _list.AddFirst(node);
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

        internal readonly struct SafeEnumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private readonly ReaderWriterLockSlim _lock;
            private readonly IEnumerator<KeyValuePair<TKey, TValue>> _inner;

            public SafeEnumerator(LruCache<TKey, TValue> cache)
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

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return TryRemove(item.Key, true, item.Value);
        }

        public int Count => Read(() => _list.Count);
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
            using (_lock.LockRead())
            {
                _list.CopyTo(array, arrayIndex);
            }
        }

        public void Add(TKey key, TValue value)
        {
            if (!TryAdd(key, value))
                throw new ArgumentException($"The key {key} already existed");
        }

        public bool ContainsKey(TKey key)
        {
            return TryGetValue(key, out _);
        }

        public bool Remove(TKey key)
        {
            return TryRemove(key, false, default);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            using var _ = _lock.LockRead();
            if (_dic.TryGetValue(key, out var node))
            {
                UpdateInternal(node);
                Stats.OnHit();
                value = node.Value.Value;
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
                    throw new KeyNotFoundException($"The given key {key} was not present");
                return value;
            }
            set => AddOrUpdate(key, value);
        }

        public ICollection<TKey> Keys => Read(() => _list.Select(m => m.Key).AsReadOnly());
        public ICollection<TValue> Values => Read(() => _list.Select(m => m.Value).AsReadOnly());

        public CacheStats Stats { get; }

        private T Read<T>(Func<T> func)
        {
            using (_lock.LockRead())
            {
                return func();
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => new SafeEnumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Dispose()
        {
            _lock?.Dispose();
        }
    }
}
