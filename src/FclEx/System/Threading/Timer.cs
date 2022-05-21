using FclEx;
using FclEx.Extensions;
using FclEx.Utils;

namespace System.Threading
{
    public delegate void TimerCallback<in T>(T state);

    public class Timer<T> : IDisposable
    {
        private Timer? _timer;

        public Timer(TimerCallback<T> callback, T state, TimeSpan dueTime, TimeSpan period)
        {
            Check.NotNull(callback);
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
