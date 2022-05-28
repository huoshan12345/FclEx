using Microsoft.Extensions.ObjectPool;
using System.Buffers;
using System.Text;

namespace FclEx.Helpers;

public static class ObjectPoolHelper
{
    public static ObjectPool<StringBuilder> StringBuilderPool { get; } = new DefaultObjectPool<StringBuilder>(new StringBuilderPooledObjectPolicy());

    public static ObjectPool<T> GetPool<T>() where T : class, new() => Cache<T>.Pool;

    public static ArrayPool<T> GetArrayPool<T>() => ArrayPool<T>.Shared;

    internal static class Cache<T> where T : class, new()
    {
        public static ObjectPool<T> Pool { get; } = new DefaultObjectPool<T>(new DefaultPooledObjectPolicy<T>());
    }
}