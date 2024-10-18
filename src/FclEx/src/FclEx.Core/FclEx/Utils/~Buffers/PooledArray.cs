namespace FclEx.Utils;

public struct PooledArray<T> : IDisposable
{
    private readonly T[] _value;
    private readonly ArrayPool<T> _pool;
    private readonly bool _clearArray;
    private bool _isDisposed;

    public PooledArray(ArrayPool<T> pool, int minimumLength, bool clearArray = false)
    {
        _clearArray = clearArray;
        _pool = Check.NotNull(pool);
        _value = pool.Rent(minimumLength);
        _isDisposed = false;
    }

    private void CheckDisposed()
    {
        if (_isDisposed)
            throw new ObjectDisposedException("The object has been disposed already.");
    }

    public T[] Value
    {
        get
        {
            CheckDisposed();
            return _value;
        }
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _pool.Return(_value, _clearArray);
        _isDisposed = true;
    }
}