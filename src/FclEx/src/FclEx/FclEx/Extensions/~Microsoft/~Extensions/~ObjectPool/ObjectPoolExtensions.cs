using Microsoft.Extensions.ObjectPool;

namespace FclEx.Extensions;

public static class ObjectPoolExtensions
{
    public static PooledObject<T> GetPooled<T>(this ObjectPool<T> pool)
        where T : class
    {
        return new PooledObject<T>(pool);
    }
}