
using System;
using System.Collections.Generic;

namespace FclEx.Utils
{
    public static class KvPair
    {
        public static KeyValuePair<TKey, TValue> Create<TKey, TValue>(TKey key, TValue value)
        {
            return new KeyValuePair<TKey, TValue>(key, value);
        }

        [Obsolete("Use " + nameof(KvPair) + "." + nameof(Create))]
        public static KeyValuePair<TKey, TValue> For<TKey, TValue>(TKey key, TValue value)
        {
            return new KeyValuePair<TKey, TValue>(key, value);
        }
    }
}
