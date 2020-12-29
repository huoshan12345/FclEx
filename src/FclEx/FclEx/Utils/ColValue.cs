using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace FclEx.Utils
{
    public class ColValue
    {
        public static ICollection<T> EmptyCol<T>() => Array.Empty<T>();
        public static IReadOnlyCollection<T> EmptyReadOnlyCol<T>() => Array.Empty<T>();
        public static IList<T> EmptyList<T>() => Array.Empty<T>();
        public static IReadOnlyList<T> EmptyReadOnlyList<T>() => Array.Empty<T>();

        public static ReadOnlyDictionary<TKey, TValue> EmptyReadOnlyDic<TKey, TValue>() where TKey : notnull
            => Cache<TKey, TValue>.EmptyDic;

        internal class Cache<TKey, TValue> where TKey : notnull
        {
            public static readonly ReadOnlyDictionary<TKey, TValue> EmptyDic
                = new(new Dictionary<TKey, TValue>());
        }
    }
}
