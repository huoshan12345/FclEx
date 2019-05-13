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

        public TimerLazy(Func<T> valueFactory, LazyThreadSafetyMode mode, TimeSpan dueTime, TimeSpan period) : base(valueFactory, mode)
        {
            _timer = NonCapturingTimer.Create<T>(o => Recreate(), default, dueTime, period);
        }

        public TimerLazy(Func<T> valueFactory, TimeSpan dueTime, TimeSpan period)
            : this(valueFactory, LazyThreadSafetyMode.None, dueTime, period)
        {
        }

        public TimerLazy(Func<T> valueFactory, LazyThreadSafetyMode mode, TimeSpan period)
            : this(valueFactory, mode, TimeSpan.Zero, period)
        {
        }

        public TimerLazy(Func<T> valueFactory, TimeSpan period)
            : this(valueFactory, LazyThreadSafetyMode.None, TimeSpan.Zero, period)
        {
        }

        public override void Dispose()
        {
            base.Dispose();
            _timer.Dispose();
        }
    }
}
