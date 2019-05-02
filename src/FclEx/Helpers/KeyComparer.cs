using System;
using System.Collections.Generic;

namespace FclEx.Helpers
{
    internal class KeyComparer<T, TKey> : IComparer<T>
    {
        private readonly Func<T, TKey> _keySelector;
        private readonly IComparer<TKey> _comparer;

        public KeyComparer(Func<T, TKey> keySelector, IComparer<TKey> comparer = null)
        {
            _keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
            _comparer = comparer ?? Comparer<TKey>.Default;
        }

        public int Compare(T x, T y)
        {
            return _comparer.Compare(_keySelector(x), _keySelector(y));
        }
    }
}