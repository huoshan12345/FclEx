namespace FclEx.Extensions;

public static class ThreadExtensions
{
    extension(Thread)
    {
        public static void SleepSafely(TimeSpan span)
        {
            if (span.Ticks <= 0)
                return;
            Thread.Sleep(span);
        }
    }
}
