using System.Threading;

namespace FclEx.Utils;

public class ReaderWriterLocker<TImpl, TIRead, TIWrite> where TImpl : TIWrite, TIRead
{
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly TImpl _shared;

    public ReaderWriterLocker(TImpl shared)
    {
        _shared = shared;
    }

    public void Read(Action<TIRead> functor)
    {
        _lock.EnterReadLock();
        try
        {
            functor(_shared);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void Write(Action<TIWrite> functor)
    {
        _lock.EnterWriteLock();
        try
        {
            functor(_shared);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
}