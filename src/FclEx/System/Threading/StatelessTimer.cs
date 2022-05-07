using Dawn;
using FclEx.Utils;

namespace System.Threading
{
    public class StatelessTimer : IDisposable
    {
        private Timer? _timer;

        public StatelessTimer(StatelessTimerCallback callback, TimeSpan dueTime, TimeSpan period)
        {
            Guard.Argument(callback, nameof(callback)).NotNull();
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
}