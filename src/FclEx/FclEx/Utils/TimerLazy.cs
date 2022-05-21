using System;
using System.Threading;

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
            : this(valueFactory, default, period, true, discardValueHandler)
        {
        }

        public override void Dispose()
        {
            base.Dispose();
            _timer.Dispose();
        }
    }
}
