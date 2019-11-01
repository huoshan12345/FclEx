using System;
using System.Threading;

namespace FclEx.Utils
{
    public class StatelessTimer : IDisposable
    {
        private Timer _timer;

        public StatelessTimer(StatelessTimerCallback callback, TimeSpan dueTime, TimeSpan period)
        {
            Check.NotNull(callback, nameof(callback));
            _timer = new Timer(s => callback(), null, dueTime, period);
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

    public class Timer<T> : IDisposable
    {
        private Timer _timer;

        public Timer(TimerCallback<T> callback, T state, TimeSpan dueTime, TimeSpan period)
        {
            Check.NotNull(callback, nameof(callback));
            _timer = new Timer(s => callback(s.CastTo<T>()), state, dueTime, period);
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
