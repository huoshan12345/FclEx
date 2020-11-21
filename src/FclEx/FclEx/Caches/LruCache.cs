using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using FclEx.Utils;

namespace FclEx.Caches
{
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public class LruCache<TKey, TValue> : IMemoryCache<TKey, TValue>
    {
        private readonly LinkedList<KeyValue> _list;
        private readonly IDictionary<TKey, LinkedListNode<KeyValue>> _dic;
        private readonly ReaderWriterLockSlim _lock;
        private static readonly IEqualityComparer<TValue> _valueComparer = EqualityComparer<TValue>.Default;
        private readonly IEqualityComparer<TKey> _keyComparer;

        public LruCache(int? capacity = null, IEqualityComparer<TKey>? comparer = null)
        {
            if (capacity.HasValue && capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            Capacity = capacity ?? ushort.MaxValue;
            _keyComparer = comparer ?? EqualityComparer<TKey>.Default;
            _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            _dic = new Dictionary<TKey, LinkedListNode<KeyValue>>(_keyComparer);
            _list = new LinkedList<KeyValue>();
            Stats = new CacheStats();
        }

        [return: MaybeNull]
        public TValue GetOrAdd(TKey key, Func<TKey, TValue> activator)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (activator == null) throw new ArgumentNullException(nameof(activator));

            using var _ = _lock.LockUpgradeableRead();

            var exist = _dic.TryGetValue(key, out var node);
            if (exist)
            {
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

        [return: MaybeNull]
        public TValue AddOrUpdate(TKey key, [AllowNull] TValue value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            using var _ = _lock.LockUpgradeableRead();

            var exist = _dic.TryGetValue(key, out var node);
            if (exist)
            {
                Debug.Assert(key.Equals(node.Value.Key));
                Stats.OnHit();
            }
            else
            {
                Stats.OnMiss();
            }

            if (!(exist && _valueComparer.Equals(node.Value.Value!, value!)))
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

        public bool TryAdd(TKey key, [AllowNull] TValue value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            using var _ = _lock.LockUpgradeableRead();

            var exist = _dic.TryGetValue(key, out var node);
            if (exist)
            {
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
#pragma warning disable CS8604 // Possible null reference argument.
            return _valueComparer.Equals(value, item.Value);
#pragma warning restore CS8604 // Possible null reference argument.
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

        public bool TryGetValue(TKey key, [NotNullWhen(true), MaybeNullWhen(false)] out TValue value)
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

        [MaybeNull]
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

        private LinkedListNode<KeyValue> UpdateInternal(LinkedListNode<KeyValue> node, [AllowNull] TValue value)
        {
            node.Value = node.Value.SetValue(value);
            return UpdateInternal(node);
        }

        private LinkedListNode<KeyValue> AddInternal(TKey key, [AllowNull] TValue value)
        {
            if (_dic.Count >= Capacity)
            {
                var toRemove = _list.Last;
                _dic.Remove(toRemove.Value.Key);
                _list.Remove(toRemove);
            }
            var node = LinkedListNodeHelper.Create(KeyValue.Create(key, value));
            _list.AddFirst(node);
            _dic[key] = node;
            return node;
        }

        private bool TryRemove(TKey key, bool matchValue, [AllowNull] TValue oldValue)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            using var _ = _lock.LockUpgradeableRead();

            var success = _dic.TryGetValue(key, out var node);
            if (success)
            {
                Debug.Assert(_keyComparer.Equals(key, node.Value.Key));

#pragma warning disable CS8604 // Possible null reference argument.
                if (matchValue && !_valueComparer.Equals(oldValue, node.Value.Value))
#pragma warning restore CS8604 // Possible null reference argument.
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
            private KeyValue(TKey key, [AllowNull] TValue value)
            {
                Key = key;
                Value = value;
            }
            public TKey Key { get; }
            [AllowNull, MaybeNull] public TValue Value { get; }
            public static KeyValue Create(TKey key, [AllowNull] TValue value) => new KeyValue(key, value);
            public KeyValue SetValue([AllowNull] TValue value) => new KeyValue(Key, value);
            public static implicit operator KeyValuePair<TKey, TValue>(KeyValue kv) => KvPair.Create(kv.Key, kv.Value!);
        }
    }
}
