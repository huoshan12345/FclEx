using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using FclEx.Utils;
using Microsoft.Extensions.ObjectPool;

namespace FclEx
{
    public static class ArrayPoolExtensions
    {
        public static PooledArray<T> GetAsDisposable<T>(this ArrayPool<T> pool, int minimumLength, bool clearArray = false)
        {
            return new PooledArray<T>(pool, minimumLength, clearArray);
        }
    }
}
