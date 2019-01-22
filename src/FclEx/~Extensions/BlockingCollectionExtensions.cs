using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using FclEx.Utils;

namespace FclEx
{
    public static class BlockingCollectionExtensions
    {
        public static void Clear<T>(this BlockingCollection<T> col)
        {
            Check.NotNull(col, nameof(col));
            while (col.TryTake(out _)) { }
        }
    }
}
