using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using FclEx.Utils;

namespace FclEx.Cache
{
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public class LruCache<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly LinkedList<KeyValuePair<TKey, TValue>> _list;
        private readonly IDictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>> _dic;
        private readonly ReaderWriterLockSlim _lock;
        private readonly int? _capacity;

        public LruCache(int? capacity = null, IEqualityComparer<TKey> comparer = null)
        {
            if (capacity > 0)
                _capacity = capacity;

            comparer = comparer ?? EqualityComparer<TKey>.Default;
            _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            _dic = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>(comparer);
            _list = new LinkedList<KeyValuePair<TKey, TValue>>();
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
            LinkedListNode<KeyValuePair<TKey, TValue>> node;
            bool success;

            _lock.EnterReadLock();
            try
            {
                success = _dic.TryGetValue(key, out node);
                if (success)
                {
                    var first = _list.First;
                    var tmp = node.Value;
                    node.Value = first.Value;
                    first.Value = tmp;
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
                    node = new LinkedListNode<KeyValuePair<TKey, TValue>>(KvPair.For(key, activator(key)));
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

        public int Count => Read(() => _list.Count);

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

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
