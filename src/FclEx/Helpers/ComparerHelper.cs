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

        public static IComparer<T> Create<T>(Comparison<T> compareFunc)
        {
            return new CommonComparer<T>(compareFunc);
        }
    }

    public static class ComparerHelper<T>
    {
        public static IComparer<T> Create<TKey>(Func<T, TKey> keySelector, IComparer<TKey> comparer = null)
        {
            return new KeyComparer<T, TKey>(keySelector, comparer);
        }

        public static IComparer<T> Create(Comparison<T> compareFunc)
        {
            return new CommonComparer<T>(compareFunc);
        }
    }
}
