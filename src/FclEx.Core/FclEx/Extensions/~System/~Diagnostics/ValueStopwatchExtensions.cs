namespace FclEx.Extensions;

public static class ValueStopwatchExtensions
{
    public static string ElapsedSeconds(this ValueStopwatch watch)
    {
        return watch.GetElapsedTime().ToSecondsString();
    }
}