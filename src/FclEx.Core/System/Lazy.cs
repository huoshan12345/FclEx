namespace System;

public static class Lazy
{
    public static Lazy<T> Create<T>(Func<T> valueFactory, bool isThreadSafe = true)
    {
        return new Lazy<T>(valueFactory, isThreadSafe);
    }

    public static TimerLazy<T> CreateTimerLazy<T>(Func<T> valueFactory, TimeSpan dueTime, TimeSpan period,
        Action<TimerLazy<T>, T>? discardValueHandler = null)
    {
        return new TimerLazy<T>(valueFactory, dueTime, period, discardValueHandler);
    }
}
