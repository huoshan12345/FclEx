using Microsoft.Extensions.ObjectPool;

namespace FclEx.Extensions;

public static class ObjectPoolExtensions
{
    public static PooledObject<T> GetAsDisposable<T>(this ObjectPool<T> pool)
        where T : class
    {
        return new PooledObject<T>(pool);
    }
}