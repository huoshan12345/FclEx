using System.Threading;

namespace FclEx.Utils;

public class ExpiringLazy<T>
{
    private readonly Func<T> _factory;
    private readonly TimeSpan _lifetime;
    private readonly ReaderWriterLockSlim _locking = new(LockRecursionPolicy.NoRecursion);
    private T? _value;
    private DateTime _expiresOn = DateTime.MinValue;

    public ExpiringLazy(Func<T> factory, TimeSpan lifetime)
    {
        _factory = factory;
        _lifetime = lifetime;
    }

    public T Value
    {
        get
        {
            var now = DateTime.UtcNow;
            _locking.EnterUpgradeableReadLock();
            try
            {
                if (_expiresOn < now)
                {
                    _locking.EnterWriteLock();
                    try
                    {
                        if (_expiresOn < now)
                        {
                            if (_value is IDisposable disposable)
                            {
                                disposable.Dispose();
                            }

                            _value = _factory();
                            _expiresOn = DateTime.UtcNow.Add(_lifetime);
                        }
                    }
                    finally
                    {
                        _locking.ExitWriteLock();
                    }
                }

                return _value!;
            }
            finally
            {
                _locking.ExitUpgradeableReadLock();
            }
        }
    }
}