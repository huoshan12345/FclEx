namespace FclEx.Helpers;

public static class ThreadHelper
{
    public static void SleepMilli(int milliSeconds)
    {
        Sleep(TimeSpan.FromMilliseconds(milliSeconds));
    }

    public static void Sleep(double seconds)
    {
        Sleep(TimeSpan.FromSeconds(seconds));
    }

    public static void Sleep(TimeSpan span)
    {
        if (span.Ticks <= 0)
            return;
        Thread.Sleep(span);
    }
}