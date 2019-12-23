using System;
using System.Collections.Generic;
using System.Text;
using Dawn;

namespace FclEx
{
    public static class ListExtensions
    {
        public static void RemoveAll<T>(this IList<T> list, Func<T, bool> filter)
        {
            Guard.Argument(list, nameof(list)).NotNull();
            Guard.Argument(filter, nameof(filter)).NotNull();

            for (var i = list.Count - 1; i >= 0; --i)
            {
                var item = list[i];
                if (filter(item))
                {
                    list.RemoveAt(i);
                }
            }
        }

        public static void Swap<T>(this IList<T> list, int left, int right)
        {
            Guard.Argument(list, nameof(list)).NotNull();
            Guard.Argument(left, nameof(left)).NotNegative().Require(m => m < list.Count);
            Guard.Argument(right, nameof(right)).NotNegative().Require(m => m < list.Count);

            var tmp = list[left];
            list[left] = list[right];
            list[right] = tmp;
        }
    }
}
