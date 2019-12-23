using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using FclEx.Utils;

namespace FclEx
{
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
    }
}
