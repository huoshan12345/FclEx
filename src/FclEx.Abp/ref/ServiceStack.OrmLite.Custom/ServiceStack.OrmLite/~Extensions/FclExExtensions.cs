using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FclEx;

namespace ServiceStack.OrmLite
{
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    internal static class FclExExtensions
    {
        public static void AddRange(this IList list, IEnumerable<object> enumerable)
        {
            Check.NotNull(list);
            Check.NotNull(enumerable);

            foreach (var obj in enumerable)
            {
                list.Add(obj);
            }
        }
    }
}
