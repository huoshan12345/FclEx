using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dawn;
using FclEx.Helpers;
using FclEx.Utils;
using MoreLinq;

namespace FclEx
{
    public static partial class EnumerableExtensions
    {
        /// <summary>
        /// Get the first item of the diff set of left and right
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool TryGetFirstOfDiffSet<T>(this ICollection<T> left, IEnumerable<T> right, [MaybeNull]out T item)
        {
            item = default;
            foreach (var check in right)
            {
                if (!left.Contains(check))
                {
                    item = check;
                    return true;
                }
            }
            return false;
        }

        public static bool TryGetFirst<T>(this IEnumerable<T>? source, [MaybeNull]out T value)
        {
            if (source != null)
            {
                using var e = source.GetEnumerator();
                if (e.MoveNext())
                {
                    value = e.Current;
                    return true;
                }
            }

            value = default;
            return false;
        }
    }
}
