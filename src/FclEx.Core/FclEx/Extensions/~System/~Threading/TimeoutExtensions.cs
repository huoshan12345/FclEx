namespace FclEx.Extensions;

public static class TimeoutExtensions
{
    private static readonly TimeSpan MaxTimeSpan = TimeSpan.FromMilliseconds(int.MaxValue);
    
    extension(Timeout)
    {
        public static TimeSpan MaxTimeSpan => MaxTimeSpan;
    }
}
