using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.ObjectPool;

namespace FclEx.Helpers
{
    public static class ObjectPoolHelper
    {
        public static ObjectPool<StringBuilder> StringBuilderPool { get; } = new DefaultObjectPool<StringBuilder>(new StringBuilderPooledObjectPolicy());

        public static ObjectPool<T> GetPool<T>() where T : class, new() => Cache<T>.Pool;

        internal static class Cache<T> where T : class, new()
        {
            public static ObjectPool<T> Pool { get; } = new DefaultObjectPool<T>(new DefaultPooledObjectPolicy<T>());
        }
    }
}
