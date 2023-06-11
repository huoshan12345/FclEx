namespace FclEx.Extensions;

public static class ReaderWriterLockSlimExtensions
{
    public static IDisposable LockRead(this ReaderWriterLockSlim locker)
    {
        locker.EnterReadLock();
        return new ActionDisposable(locker.ExitReadLock);
    }

    public static IDisposable LockWrite(this ReaderWriterLockSlim locker)
    {
        locker.EnterWriteLock();
        return new ActionDisposable(locker.ExitWriteLock);
    }

    public static IDisposable LockUpgradeableRead(this ReaderWriterLockSlim locker)
    {
        locker.EnterUpgradeableReadLock();
        return new ActionDisposable(locker.ExitUpgradeableReadLock);
    }
}