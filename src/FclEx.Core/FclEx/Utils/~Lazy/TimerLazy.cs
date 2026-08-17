namespace FclEx.Utils;

public class TimerLazy<T> : ReLazy<TimerLazy<T>, T>
{
    private readonly Timer _timer;
    private readonly object _timerCallbackLock = new();

    public TimerLazy(Func<T> valueFactory, TimeSpan dueTime, TimeSpan period,
        Action<TimerLazy<T>, T>? discardValueHandler = null)
        : base(valueFactory, discardValueHandler)
    {
        _timer = NonCapturingTimer.Create(OnTimer, dueTime, period);
    }

    public TimerLazy(Func<T> valueFactory, TimeSpan period, Action<TimerLazy<T>, T>? discardValueHandler = null)
        : this(valueFactory, default, period, discardValueHandler)
    {
    }

    public override void Dispose()
    {
        _timer.Dispose();

        lock (_timerCallbackLock)
        {
            base.Dispose();
        }
    }

    private void OnTimer()
    {
        lock (_timerCallbackLock)
        {
            TryRecreate();
        }
    }
}
