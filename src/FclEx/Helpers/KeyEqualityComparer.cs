using System;
using System.Collections.Generic;

namespace FclEx.Helpers
{
    internal class KeyEqualityComparer<T, TKey> : IEqualityComparer<T>
    {
        private readonly Func<T, TKey> _keySelector;
        private readonly IEqualityComparer<TKey> _comparer;

        public KeyEqualityComparer(Func<T, TKey> keySelector, IEqualityComparer<TKey>? comparer)
        {
            _keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
            _comparer = comparer ?? EqualityComparer<TKey>.Default;
        }

        public bool Equals(T x, T y)
        {
            return _comparer.Equals(_keySelector(x), _keySelector(y));
        }

        public int GetHashCode(T obj)
        {
            return _comparer.GetHashCode(_keySelector(obj));
        }
    }
}