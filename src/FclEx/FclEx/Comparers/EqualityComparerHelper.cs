using System;
using System.Collections.Generic;

namespace FclEx.Comparers
{
    public static class EqualityComparerHelper
    {
        public static IEqualityComparer<T> Create<T>(Func<T?, T?, bool> compareFunc, Func<T, int> hashFunc)
        {
            return new CommonEqualityComparer<T>(compareFunc, hashFunc);
        }

        public static IEqualityComparer<T> Create<T, TKey>(Func<T?, TKey> keySelector,
            IEqualityComparer<TKey>? comparer = null)
        {
            return new KeyEqualityComparer<T, TKey>(keySelector, comparer);
        }
    }

    public static class EqualityComparerHelper<T>
    {
        public static IEqualityComparer<T> Create(Func<T?, T?, bool> compareFunc, Func<T, int> hashFunc)
        {
            return new CommonEqualityComparer<T>(compareFunc, hashFunc);
        }

        public static IEqualityComparer<T> Create<TKey>(Func<T?, TKey> keySelector,
            IEqualityComparer<TKey>? comparer = null)
        {
            return new KeyEqualityComparer<T, TKey>(keySelector, comparer);
        }
    }
}
