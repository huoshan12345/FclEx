using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using FclEx.Helpers;

namespace FclEx.Utils
{
    public class TimerLazy<T> : ReLazy<T>
    {
        private readonly StatelessTimer _timer;

        public TimerLazy(Func<T> valueFactory, LazyThreadSafetyMode mode, TimeSpan dueTime, TimeSpan period, EventHandler<T> discardValueHandler = null)
            : base(valueFactory, mode, discardValueHandler)
        {
            _timer = NonCapturingTimer.Create(Recreate, dueTime, period);
        }

        public TimerLazy(Func<T> valueFactory, TimeSpan dueTime, TimeSpan period, EventHandler<T> discardValueHandler = null)
            : this(valueFactory, LazyThreadSafetyMode.ExecutionAndPublication, dueTime, period, discardValueHandler)
        {
        }

        public TimerLazy(Func<T> valueFactory, LazyThreadSafetyMode mode, TimeSpan period, EventHandler<T> discardValueHandler = null)
            : this(valueFactory, mode, period, period, discardValueHandler)
        {
        }

        public TimerLazy(Func<T> valueFactory, TimeSpan period, EventHandler<T> discardValueHandler = null)
            : this(valueFactory, LazyThreadSafetyMode.ExecutionAndPublication, period, period, discardValueHandler)
        {
        }

        public override void Dispose()
        {
            base.Dispose();
            _timer.Dispose();
        }
    }
}
