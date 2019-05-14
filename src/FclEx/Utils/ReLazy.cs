using System;
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
        protected readonly bool _disposable = typeof(IDisposable).IsAssignableFrom(typeof(T));
        protected readonly bool _disposeObj;

        public ReLazy(Func<T> valueFactory, LazyThreadSafetyMode mode, bool disposeObj = false)
        {
            _valueFactory = valueFactory;
            _mode = mode;
            _disposeObj = disposeObj;
            _lazy = new Lazy<T>(_valueFactory, _mode);
        }

        public ReLazy(Func<T> valueFactory, bool disposeObj = false)
            : this(valueFactory, LazyThreadSafetyMode.None, disposeObj)
        {
        }

        public T Value => _lazy.Value;
        public bool IsValueCreated => _lazy.IsValueCreated;
        
        public void SetValueFactory(Func<T> valueFactory)
        {
            LockHelper.DoubleCheckAndDo(() => _valueFactory != valueFactory, _lock, () =>
            {
                DisposeObj();
                _valueFactory = valueFactory;
                _lazy = new Lazy<T>(valueFactory, _mode);
            });
        }

        public void Recreate()
        {
            LockHelper.DoubleCheckAndDo(() => _lazy.IsValueCreated, _lock, () =>
            {
                DisposeObj();
                _lazy = new Lazy<T>(_valueFactory, _mode);
            });
        }

        protected virtual void DisposeObj()
        {
            if (_lazy.IsValueCreated && _disposable && _disposeObj)
                ((IDisposable)_lazy.Value).Dispose();
        }

        public virtual void Dispose()
        {
            DisposeObj();
        }
    }
}
