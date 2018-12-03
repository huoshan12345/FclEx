using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using FclEx.Helpers;

namespace FclEx.Utils
{
    public class TimerLazy<T> : IDisposable
    {
        private readonly object _lock = new object();
        private volatile Lazy<T> _lazy;
        private readonly Timer _timer;
        private volatile Func<T> _valueFactory;
        private readonly LazyThreadSafetyMode _mode;

        public TimerLazy(Func<T> valueFactory, LazyThreadSafetyMode mode, TimeSpan span)
        {
            _valueFactory = valueFactory;
            _mode = mode;

            _lazy = new Lazy<T>(_valueFactory, _mode);
            _timer = new Timer(o => Recreate(), null, span, span);
        }

        public TimerLazy(Func<T> valueFactory, TimeSpan span)
            : this(valueFactory, LazyThreadSafetyMode.None, span)
        {
        }

        public T Value => _lazy.Value;

        public void SetValueFactory(Func<T> valueFactory)
        {
            LockHelper.DoubleCheckAndDo(() => _valueFactory != valueFactory, _lock, () =>
             {
                 if (_lazy.IsValueCreated && _lazy.Value is IDisposable disposable)
                     disposable.Dispose();
                 _valueFactory = valueFactory;
                 _lazy = new Lazy<T>(valueFactory, _mode);
             });
        }

        public void Recreate()
        {
            LockHelper.DoubleCheckAndDo(() => _lazy.IsValueCreated, _lock, () =>
            {
                if (_lazy.Value is IDisposable disposable)
                    disposable.Dispose();
                _lazy = new Lazy<T>(_valueFactory, _mode);
            });
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}
