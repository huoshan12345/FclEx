using System;
using System.Collections.Generic;
using System.Threading;
using FclEx.Helpers;

namespace FclEx.Utils
{
    public class ReLazy<TSelf, T> : IDisposable where TSelf : ReLazy<TSelf, T>
    {
        protected readonly object _lock = new object();
        protected volatile Lazy<T> _lazy;
        protected volatile Func<T> _valueFactory;
        protected readonly bool _isThreadSafe;

        public event EventHandler<TSelf, T> OnDiscardValue = (sender, e) => { };

        public ReLazy(Func<T> valueFactory, bool isThreadSafe = true, EventHandler<TSelf, T> discardValueHandler = null)
        {
            _valueFactory = valueFactory;
            _isThreadSafe = isThreadSafe;
            _lazy = new Lazy<T>(_valueFactory, isThreadSafe);
            OnDiscardValue += discardValueHandler;
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
            if (_lazy.IsValueCreated)
                OnDiscardValue((TSelf)this, _lazy.Value);
        }

        public virtual void Dispose()
        {
            DiscardValue();
        }
    }

    public class ReLazy<T> : ReLazy<ReLazy<T>, T>
    {
        public ReLazy(Func<T> valueFactory, bool isThreadSafe = true, EventHandler<ReLazy<T>, T> discardValueHandler = null) 
            : base(valueFactory, isThreadSafe, discardValueHandler)
        {
        }
    }
}
