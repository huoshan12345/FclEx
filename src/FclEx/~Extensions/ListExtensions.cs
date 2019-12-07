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
    }
}
