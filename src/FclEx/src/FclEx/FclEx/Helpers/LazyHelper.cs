namespace FclEx.Helpers;

public static class LazyHelper
{
    public static Lazy<T> Create<T>(Func<T> valueFactory, bool isThreadSafe = true)
    {
        return new Lazy<T>(valueFactory, isThreadSafe);
    }

    public static TimerLazy<T> CreateTime<T>(Func<T> valueFactory, TimeSpan dueTime, TimeSpan period,
        bool isThreadSafe = true, Action<TimerLazy<T>, T>? discardValueHandler = null)
    {
        return new TimerLazy<T>(valueFactory, dueTime, period, isThreadSafe, discardValueHandler);
    }
}