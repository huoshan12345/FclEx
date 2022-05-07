using System.Collections.Generic;

namespace FclEx
{
    public static class SetExtensions
    {
        public static void AddRange<T>(this ISet<T> set, IEnumerable<T> items)
        {
            foreach (var item in items)
                set.Add(item);
        }
    }
}
