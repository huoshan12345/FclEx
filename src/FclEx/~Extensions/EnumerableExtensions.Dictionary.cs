using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dawn;
using FclEx.Helpers;
using FclEx.Utils;
using Microsoft.Collections.Extensions;
using MoreLinq;

namespace FclEx
{
    partial class EnumerableExtensions
    {
        public static MultiValueDictionary<TKey, TValue> ToMultiValueDic<T, TKey, TValue>(this IEnumerable<T> enumerable,
            Func<T, TKey> keySelector, Func<T, IReadOnlyCollection<TValue>> valueSelector)
        {
            return new MultiValueDictionary<TKey, TValue>(enumerable.Select(m => KvPair.For(keySelector(m), valueSelector(m))));
        }

        public static MultiValueDictionary<TKey, TValue> ToMultiValueDic<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> enumerable)
        {
            var col = enumerable.GroupBy(m => m.Key)
                .Select(m => KvPair.For(m.Key, (IReadOnlyCollection<TValue>)m.Select(x => x.Value).ToArray()));
            return new MultiValueDictionary<TKey, TValue>(col);
        }

        public static OrderedDictionary<TKey, TValue> ToOrderedDic<T, TKey, TValue>(this IEnumerable<T> enumerable,
            Func<T, TKey> keySelector, Func<T, TValue> valueSelector)
        {
            return new OrderedDictionary<TKey, TValue>(enumerable.Select(m => KvPair.For(keySelector(m), valueSelector(m))));
        }

        public static OrderedDictionary<TKey, TValue> ToOrderedDic<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> enumerable)
        {
            return new OrderedDictionary<TKey, TValue>(enumerable);
        }

    }
}
