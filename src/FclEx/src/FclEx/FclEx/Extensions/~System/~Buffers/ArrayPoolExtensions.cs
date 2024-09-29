namespace FclEx.Extensions;

public static class ArrayPoolExtensions
{
    public static PooledArray<T> GetPooled<T>(this ArrayPool<T> pool, int minimumLength, bool clearArray = false)
    {
        return new PooledArray<T>(pool, minimumLength, clearArray);
    }
}