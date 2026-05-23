namespace FclEx.Extensions;

public static class ReaderWriterLockSlimExtensions
{
    public static IDisposable LockRead(this ReaderWriterLockSlim locker)
    {
        locker.EnterReadLock();
        return new Disposable(locker.ExitReadLock);
    }

    public static IDisposable LockWrite(this ReaderWriterLockSlim locker)
    {
        locker.EnterWriteLock();
        return new Disposable(locker.ExitWriteLock);
    }

    public static IDisposable LockUpgradeableRead(this ReaderWriterLockSlim locker)
    {
        locker.EnterUpgradeableReadLock();
        return new Disposable(locker.ExitUpgradeableReadLock);
    }
}