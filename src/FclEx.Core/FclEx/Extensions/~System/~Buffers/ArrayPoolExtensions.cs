namespace FclEx.Extensions;

public static class ArrayPoolExtensions
{
    public static DisposableValue<T[]> GetPooled<T>(this ArrayPool<T> pool, int minimumLength, bool clearArray = false)
    {
        return Disposable.FromValue(pool.Rent(minimumLength), m => pool.Return(m, clearArray));
    }
}