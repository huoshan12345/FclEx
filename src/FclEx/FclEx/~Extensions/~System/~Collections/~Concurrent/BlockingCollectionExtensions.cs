using System.Collections.Concurrent;
using System.Collections.Generic;
using Dawn;

namespace FclEx
{
    public static class BlockingCollectionExtensions
    {
        public static void Clear<T>(this BlockingCollection<T> col)
        {
            Guard.Argument(col, nameof(col)).NotNull();
            while (col.TryTake(out _)) { }
        }

        public static List<T> TakeAll<T>(this BlockingCollection<T> col)
        {
            Guard.Argument(col, nameof(col)).NotNull();
            var list = new List<T>();
            while (col.TryTake(out var item))
                list.Add(item);
            return list;
        }
    }
}
