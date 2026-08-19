namespace FclEx.Utils;

public class TimerLazy<T> : ReLazy<TimerLazy<T>, T>
{
    private readonly Timer _timer;
    private static readonly
#if NET9_0_OR_GREATER
        Lock
#else
        object
#endif
    _lock = new();

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
        GC.SuppressFinalize(this);

        _timer.Dispose();

        lock (_lock)
        {
            base.Dispose();
        }
    }

    private void OnTimer()
    {
        lock (_lock)
        {
            TryRecreate();
        }
    }
}
