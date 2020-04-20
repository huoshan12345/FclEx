using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using FclEx.Helpers;

namespace FclEx.Utils
{
    public class TimerLazy<T> : ReLazy<TimerLazy<T>, T>
    {
        private readonly StatelessTimer _timer;

        public TimerLazy(Func<T> valueFactory, TimeSpan dueTime, TimeSpan period, bool isThreadSafe = true,
            Action<TimerLazy<T>, T>? discardValueHandler = null)
            : base(valueFactory, isThreadSafe, discardValueHandler)
        {
            _timer = NonCapturingTimer.Create(Recreate, dueTime, period);
        }

        public TimerLazy(Func<T> valueFactory, TimeSpan period, Action<TimerLazy<T>, T>? discardValueHandler = null)
            : this(valueFactory, period, period, true, discardValueHandler)
        {
        }

        public override void Dispose()
        {
            base.Dispose();
            _timer.Dispose();
        }
    }
}
