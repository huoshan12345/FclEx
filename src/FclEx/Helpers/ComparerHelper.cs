using System;
using System.Collections.Generic;
using System.Reflection;

namespace FclEx.Helpers
{
    public static class ComparerHelper
    {
        public static IComparer<T> Create<T, TKey>(Func<T, TKey> keySelector, IComparer<TKey> comparer = null)
        {
            return new KeyComparer<T, TKey>(keySelector, comparer);
        }

        private class KeyComparer<T, TKey> : IComparer<T>
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

        public static IComparer<T> Create<T>(Comparison<T> compareFunc)
        {
            return new CommonComparer<T>(compareFunc);
        }

        private class CommonComparer<T> : IComparer<T>
        {
            private readonly Comparison<T> _compareFunc;

            public CommonComparer(Comparison<T> compareFunc)
            {
                _compareFunc = compareFunc ?? throw new ArgumentNullException(nameof(compareFunc));
            }

            public int Compare(T x, T y)
            {
                return _compareFunc(x, y);
            }
        }
    }
}
