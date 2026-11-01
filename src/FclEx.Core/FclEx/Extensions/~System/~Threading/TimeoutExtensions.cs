namespace FclEx.Extensions;

public static class TimeoutExtensions
{
    private static readonly TimeSpan MaxTimeSpan = TimeSpan.FromMilliseconds(uint.MaxValue - 1L);

    extension(Timeout)
    {
        public static TimeSpan MaxTimeSpan => MaxTimeSpan;
    }
}
