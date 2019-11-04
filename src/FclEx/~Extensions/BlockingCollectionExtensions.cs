using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Dawn;
using FclEx.Utils;

namespace FclEx
{
    public static class BlockingCollectionExtensions
    {
        public static void Clear<T>(this BlockingCollection<T> col)
        {
            Guard.Argument(col, nameof(col)).NotNull();
            while (col.TryTake(out _)) { }
        }
    }
}
