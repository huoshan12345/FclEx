using System;
using System.Collections.Generic;
using System.Threading;
using FclEx.Helpers;

namespace FclEx.Utils
{
    public class ReLazy<T> : IDisposable
    {
        protected readonly object _lock = new object();
        protected volatile Lazy<T> _lazy;
        protected volatile Func<T> _valueFactory;
        protected readonly LazyThreadSafetyMode _mode;

        public event EventHandler<T> OnDiscardValue = t => { };

        public ReLazy(Func<T> valueFactory, LazyThreadSafetyMode mode, EventHandler<T> discardValueHandler = null)
        {
            _valueFactory = valueFactory;
            _mode = mode;
            _lazy = new Lazy<T>(_valueFactory, _mode);
            OnDiscardValue += discardValueHandler;
        }

        public ReLazy(Func<T> valueFactory, EventHandler<T> discardValueHandler = null)
            : this(valueFactory, LazyThreadSafetyMode.ExecutionAndPublication, discardValueHandler)
        {
        }

        public T Value => _lazy.Value;
        public bool IsValueCreated => _lazy.IsValueCreated;

        public void SetValueFactory(Func<T> valueFactory)
        {
            LockHelper.DoubleCheckAndDo(() => _valueFactory != valueFactory, _lock, () =>
            {
                DiscardValue();
                _valueFactory = valueFactory;
                _lazy = new Lazy<T>(valueFactory, _mode);
            });
        }

        public void Recreate()
        {
            LockHelper.DoubleCheckAndDo(() => _lazy.IsValueCreated, _lock, () =>
            {
                DiscardValue();
                _lazy = new Lazy<T>(_valueFactory, _mode);
            });
        }

        protected virtual void DiscardValue()
        {
            if (_lazy.IsValueCreated)
                OnDiscardValue(_lazy.Value);
        }

        public virtual void Dispose()
        {
            DiscardValue();
        }
    }
}
