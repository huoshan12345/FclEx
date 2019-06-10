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
    public sealed class CounterCache<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly LinkedList<KvCount> _list;
        private readonly IDictionary<TKey, LinkedListNode<KvCount>> _dic;
        private readonly ReaderWriterLockSlim _lock;
        private readonly int? _capacity;

        public CounterCache(int? capacity = null, IEqualityComparer<TKey> comparer = null)
        {
            if (capacity > 0)
                _capacity = capacity;

            comparer = comparer ?? EqualityComparer<TKey>.Default;
            _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            _dic = new Dictionary<TKey, LinkedListNode<KvCount>>(comparer);
            _list = new LinkedList<KvCount>();
        }

        public bool TryGet(TKey key, out TValue value)
        {
            _lock.EnterReadLock();
            try
            {
                if (_dic.TryGetValue(key, out var node))
                {
                    value = node.Value.Value;
                    return true;
                }
                else
                {
                    value = default;
                    return false;
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> activator)
        {
            LinkedListNode<KvCount> node;
            bool success;

            _lock.EnterReadLock();
            try
            {
                success = _dic.TryGetValue(key, out node);
                if (success)
                {
                    node.Value.Incre();
                    var cur = node;
                    while (true)
                    {
                        var prev = cur.Previous;
                        if (prev == null) break;

                        var count = cur.Value.Count;
                        var pCount = prev.Value.Count;
                        if (count <= pCount)
                            break;

                        var tmp = cur.Value;
                        cur.Value = prev.Value;
                        prev.Value = tmp;
                        cur = prev;
                    }
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }

            if (!success)
            {
                _lock.EnterWriteLock();
                try
                {
                    if (_capacity.HasValue)
                    {
                        if (_dic.Count >= _capacity)
                        {
                            var last = _list.Last;
                            _dic.Remove(last.Value.Key);
                            _list.RemoveLast();
                        }
                    }
                    node = new LinkedListNode<KvCount>(new KvCount(key, activator(key)));
                    _list.AddLast(node);
                    _dic[key] = node;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            return node.Value.Value;
        }

        public int Count => Read(() => _dic.Count);

        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                _list.Clear();
                _dic.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public IReadOnlyList<TKey> GetAllKeys() => Read(() => _list.Select(m => m.Key).ToArray());

        public bool IsFull() => Read(() => _dic.Count >= _capacity);

        internal class KvCount
        {
            public KvCount(TKey key, TValue value)
            {
                Key = key;
                Value = value;
            }

            public TKey Key { get; }
            public TValue Value { get; }
            public int Count { get; private set; }
            public int Incre() => ++Count;

            public void Deconstruct(out TKey key, out TValue value, out int count)
            {
                key = Key;
                value = Value;
                count = Count;
            }
        }

        private T Read<T>(Func<T> func)
        {
            _lock.EnterReadLock();
            try
            {
                return func();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Enumerator
        /// </summary>
        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private readonly CounterCache<TKey, TValue> _dictionary;
            private LinkedListNode<KvCount> _node;

            internal Enumerator(CounterCache<TKey, TValue> dictionary)
            {
                _dictionary = dictionary;
                _node = default;
            }

            /// <summary>
            /// Move to next
            /// </summary>
            public bool MoveNext()
            {
                var list = _dictionary._list;
                _node = _node == null ? list.First : _node.Next;
                if (_node == null)
                {
                    Current = default;
                    return false;
                }
                else
                {
                    Current = GetCur();
                    return true;
                }
            }

            private KeyValuePair<TKey, TValue> GetCur()
            {
                return KvPair.For(_node.Value.Key, _node.Value.Value);
            }

            /// <summary>
            /// Get current value
            /// </summary>
            public KeyValuePair<TKey, TValue> Current { get; private set; }

            object IEnumerator.Current => _node;

            void IEnumerator.Reset()
            {
                Current = default;
                _node = default;
            }

            /// <summary>
            /// Dispose the enumerator
            /// </summary>
            public void Dispose() { }
        }
    }
}
