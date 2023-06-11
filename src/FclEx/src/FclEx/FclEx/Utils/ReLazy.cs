using System.Collections.Generic;
using FclEx.Helpers;

namespace FclEx.Utils;

public class ReLazy<TSelf, T> : IDisposable where TSelf : ReLazy<TSelf, T>
{
    protected readonly object _lock = new();
    protected volatile Lazy<T> _lazy;
    protected volatile Func<T> _valueFactory;
    protected readonly bool _isThreadSafe;

    private readonly Action<TSelf, T>? _onDiscardValue;

    public ReLazy(Func<T> valueFactory, bool isThreadSafe = true, Action<TSelf, T>? discardValueHandler = null)
    {
        _valueFactory = valueFactory;
        _isThreadSafe = isThreadSafe;
        _lazy = new Lazy<T>(_valueFactory, isThreadSafe);
        _onDiscardValue = discardValueHandler;
    }

    public T Value => _lazy.Value;
    public bool IsValueCreated => _lazy.IsValueCreated;

    public void SetValueFactory(Func<T> valueFactory)
    {
        LockHelper.DoubleCheckAndDo(() => _valueFactory != valueFactory, _lock, () =>
        {
            DiscardValue();
            _valueFactory = valueFactory;
            _lazy = new Lazy<T>(valueFactory, _isThreadSafe);
        });
    }

    public void Recreate()
    {
        LockHelper.DoubleCheckAndDo(() => _lazy.IsValueCreated, _lock, () =>
        {
            DiscardValue();
            _lazy = new Lazy<T>(_valueFactory, _isThreadSafe);
        });
    }

    protected virtual void DiscardValue()
    {
        if (_lazy.IsValueCreated && _onDiscardValue != null)
            _onDiscardValue((TSelf)this, _lazy.Value);
    }

    public virtual void Dispose()
    {
        DiscardValue();
    }
}

public class ReLazy<T> : ReLazy<ReLazy<T>, T>
{
    public ReLazy(Func<T> valueFactory, bool isThreadSafe = true, Action<ReLazy<T>, T>? discardValueHandler = null)
        : base(valueFactory, isThreadSafe, discardValueHandler)
    {
    }
}