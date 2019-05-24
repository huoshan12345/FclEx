using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using FclEx.Helpers;

namespace FclEx.Utils
{
    public class TimerLazy<T> : ReLazy<T>
    {
        private readonly Timer<T> _timer;

        public TimerLazy(Func<T> valueFactory, LazyThreadSafetyMode mode, TimeSpan dueTime, TimeSpan period, bool disposeObj = false) : base(valueFactory, mode, disposeObj)
        {
            _timer = NonCapturingTimer.Create<T>(o => Recreate(), default, dueTime, period);
        }

        public TimerLazy(Func<T> valueFactory, TimeSpan dueTime, TimeSpan period, bool disposeObj = false)
            : this(valueFactory, LazyThreadSafetyMode.None, dueTime, period, disposeObj)
        {
        }

        public TimerLazy(Func<T> valueFactory, LazyThreadSafetyMode mode, TimeSpan period, bool disposeObj = false)
            : this(valueFactory, mode, period, period, disposeObj)
        {
        }

        public TimerLazy(Func<T> valueFactory, TimeSpan period, bool disposeObj = false)
            : this(valueFactory, LazyThreadSafetyMode.None, period, period, disposeObj)
        {
        }

        public override void Dispose()
        {
            base.Dispose();
            _timer.Dispose();
        }
    }
}
