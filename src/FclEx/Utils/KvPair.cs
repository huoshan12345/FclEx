
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FclEx.Utils
{
    public static class KvPair
    {
        public static KeyValuePair<TKey, TValue> Create<TKey, TValue>([AllowNull] TKey key, [AllowNull] TValue value)
        {
#pragma warning disable 8604
            return new KeyValuePair<TKey, TValue>(key, value);
#pragma warning restore 8604
        }

        [Obsolete("Use " + nameof(KvPair) + "." + nameof(Create))]
        public static KeyValuePair<TKey, TValue> For<TKey, TValue>(TKey key, TValue value)
        {
            return new KeyValuePair<TKey, TValue>(key, value);
        }
    }
}
