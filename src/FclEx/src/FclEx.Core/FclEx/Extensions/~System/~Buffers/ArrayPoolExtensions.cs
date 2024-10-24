namespace FclEx.Extensions;

public static class ArrayPoolExtensions
{
    public static DisposableValue<T[]> GetPooled<T>(this ArrayPool<T> pool, int minimumLength, bool clearArray = false)
    {
        return pool.Rent(minimumLength).ToDisposable(m => pool.Return(m, clearArray));
    }
}