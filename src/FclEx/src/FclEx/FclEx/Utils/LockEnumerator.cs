using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace FclEx.Utils;

public class LockEnumerator
{
    public static LockEnumerator<T> Create<T>(IEnumerator<T> inner, ReaderWriterLockSlim @lock) => new(inner, @lock);
}

public struct LockEnumerator<T> : IEnumerator<T>
{
    private readonly ReaderWriterLockSlim _lock;
    private readonly IEnumerator<T> _inner;
    private bool _isDisposed;

    internal LockEnumerator(IEnumerator<T> inner, ReaderWriterLockSlim @lock)
    {
        _isDisposed = false;
        _inner = inner;
        _lock = @lock;
        _lock.EnterReadLock();
    }

    public bool MoveNext()
    {
        return _inner.MoveNext();
    }

    public void Reset()
    {
        _inner.Reset();
    }

    public T Current => _inner.Current;

    object IEnumerator.Current => Current!;

    public void Dispose()
    {
        if (!_isDisposed)
        {
            _isDisposed = true;
            _lock.ExitReadLock();
        }
    }
}