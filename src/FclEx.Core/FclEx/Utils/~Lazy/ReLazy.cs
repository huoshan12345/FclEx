namespace FclEx.Utils;

public class ReLazy<TSelf, T> : IDisposable where TSelf : ReLazy<TSelf, T>
{
    private readonly
#if NET9_0_OR_GREATER
        Lock
#else
        object
#endif
    _lock = new();
    private readonly Action<TSelf, T>? _discardValueHandler;

    private Lazy<T> _lazy;
    private Func<T> _valueFactory;
    private bool _isDisposed;

    public ReLazy(Func<T> valueFactory, Action<TSelf, T>? discardValueHandler = null)
    {
        _valueFactory = Check.NotNull(valueFactory);
        _lazy = CreateLazy(valueFactory);
        _discardValueHandler = discardValueHandler;
    }

    public T Value
    {
        get
        {
            lock (_lock)
            {
                ThrowIfDisposed();
                return _lazy.Value;
            }
        }
    }

    public bool IsValueCreated
    {
        get
        {
            lock (_lock)
            {
                ThrowIfDisposed();
                return _lazy.IsValueCreated;
            }
        }
    }

    public void SetValueFactory(Func<T> valueFactory)
    {
        Check.NotNull(valueFactory);

        Lazy<T>? discardedValue = null;
        lock (_lock)
        {
            ThrowIfDisposed();
            if (_valueFactory == valueFactory)
                return;

            if (_lazy.IsValueCreated)
                discardedValue = _lazy;

            _valueFactory = valueFactory;
            _lazy = CreateLazy(valueFactory);
        }

        DiscardValue(discardedValue);
    }

    public void Recreate()
    {
        var discardedValue = ReplaceCreatedValue(throwIfDisposed: true);
        DiscardValue(discardedValue);
    }

    protected bool TryRecreate()
    {
        var discardedValue = ReplaceCreatedValue(throwIfDisposed: false);
        if (discardedValue is null)
            return false;

        DiscardValue(discardedValue);
        return true;
    }

    private Lazy<T>? ReplaceCreatedValue(bool throwIfDisposed)
    {
        lock (_lock)
        {
            if (_isDisposed)
            {
                if (throwIfDisposed)
                    ThrowIfDisposed();

                return null;
            }

            if (_lazy.IsValueCreated == false)
                return null;

            var discardedValue = _lazy;
            _lazy = CreateLazy(_valueFactory);
            return discardedValue;
        }
    }

    private static Lazy<T> CreateLazy(Func<T> valueFactory) => new(valueFactory, true);

    private void DiscardValue(Lazy<T>? lazy)
    {
        if (lazy is not null)
            _discardValueHandler?.Invoke((TSelf)this, lazy.Value);
    }

    public virtual void Dispose()
    {
        Lazy<T>? discardedValue;
        lock (_lock)
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            discardedValue = _lazy.IsValueCreated ? _lazy : null;
        }

        try
        {
            DiscardValue(discardedValue);
        }
        finally
        {
            GC.SuppressFinalize(this);
        }
    }

    private void ThrowIfDisposed()
    {
        if (_isDisposed)
            throw new ObjectDisposedException(GetType().FullName);
    }
}

public class ReLazy<T> : ReLazy<ReLazy<T>, T>
{
    public ReLazy(Func<T> valueFactory, Action<ReLazy<T>, T>? discardValueHandler = null)
        : base(valueFactory, discardValueHandler)
    {
    }
}
