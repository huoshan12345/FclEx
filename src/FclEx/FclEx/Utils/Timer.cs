using System;
using System.Threading;
using Dawn;

namespace FclEx.Utils
{
    public class Timer<T> : IDisposable
    {
        private Timer? _timer;

        public Timer(TimerCallback<T> callback, T state, TimeSpan dueTime, TimeSpan period)
        {
            Guard.Argument(callback, nameof(callback)).NotNull();
            _timer = new Timer(s => callback(s.CastTo<T>()!), state, dueTime, period);
        }

        public void Dispose()
        {
            if (_timer == null)
                return;

            _timer.Dispose();
            _timer = null;
        }

        public bool Available => _timer != null;
    }
}
