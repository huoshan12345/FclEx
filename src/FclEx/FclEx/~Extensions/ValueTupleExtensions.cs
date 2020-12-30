using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FclEx.Helpers;
using FclEx.Utils;

namespace FclEx
{
    public static class ValueTupleExtensions
    {
        public static (T1, T2) ToValue<T1, T2>(this Tuple<T1, T2> tuple)
            => (tuple.Item1, tuple.Item2);

        public static (T1, T2, T3) ToValue<T1, T2, T3>(this Tuple<T1, T2, T3> tuple)
            => (tuple.Item1, tuple.Item2, tuple.Item3);

        public static (T1, T2, T3, T4) ToValue<T1, T2, T3, T4>(this Tuple<T1, T2, T3, T4> tuple)
            => (tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);

        public static IEnumerable<(T1, T2)> ToValue<T1, T2>(this IEnumerable<Tuple<T1, T2>> enumerable)
            => enumerable.Select(m => m.ToValue());

        public static IEnumerable<(T1, T2, T3)> ToValue<T1, T2, T3>(this IEnumerable<Tuple<T1, T2, T3>> enumerable)
            => enumerable.Select(m => m.ToValue());

        public static IEnumerable<(T1, T2, T3, T4)> ToValue<T1, T2, T3, T4>(this IEnumerable<Tuple<T1, T2, T3, T4>> enumerable)
            => enumerable.Select(m => m.ToValue());

        public static KeyValuePair<T1, T2> AsKeyValue<T1, T2>(this ValueTuple<T1, T2> tuple)
        {
            return KvPair.Create(tuple.Item1, tuple.Item2);
        }

        public static string FirstValid(this IEnumerable<string?> values, int? count = null, string defaultValue = "")
        {
            var q = values;
            if (count.HasValue)
                q = q.Take(count.Value);
            return q.FirstOrDefault(m => m.IsValid()) ?? defaultValue;
        }

        public static string FirstValid(this (string?, string?) tuple, string defaultValue = "")
        {
            const int count = 2;
            using var disposable = ObjectPoolHelper.GetArrayPool<string?>().GetAsDisposable(count);
            var arr = disposable.Value;
            arr[0] = tuple.Item1;
            arr[1] = tuple.Item2;
            return arr.FirstValid(count, defaultValue);
        }

        public static string FirstValid(this (string?, string?, string?) tuple, string defaultValue = "")
        {
            const int count = 3;
            using var disposable = ObjectPoolHelper.GetArrayPool<string?>().GetAsDisposable(count);
            var arr = disposable.Value;
            arr[0] = tuple.Item1;
            arr[1] = tuple.Item2;
            arr[2] = tuple.Item3;
            return arr.FirstValid(count, defaultValue);
        }

        public static string FirstValid(this (string?, string?, string?, string?) tuple, string defaultValue = "")
        {
            const int count = 4;
            using var disposable = ObjectPoolHelper.GetArrayPool<string?>().GetAsDisposable(count);
            var arr = disposable.Value;
            arr[0] = tuple.Item1;
            arr[1] = tuple.Item2;
            arr[2] = tuple.Item3;
            arr[3] = tuple.Item4;
            return arr.FirstValid(count, defaultValue);
        }
    }
}
