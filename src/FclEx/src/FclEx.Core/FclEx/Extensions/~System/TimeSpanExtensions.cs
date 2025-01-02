namespace FclEx.Extensions;

public static class TimeSpanExtensions
{
    /// <summary>
    /// Multiplies a timespan by an integer value
    /// </summary>
    public static TimeSpan Multiply(this TimeSpan multiplicand, int multiplier)
    {
        return TimeSpan.FromTicks(multiplicand.Ticks * multiplier);
    }

    /// <summary>
    /// Multiplies a timespan by a double value
    /// </summary>
    public static TimeSpan Multiply(this TimeSpan multiplicand, double multiplier)
    {
        return TimeSpan.FromTicks((long)(multiplicand.Ticks * multiplier));
    }

    public static string ToSecondsString(this TimeSpan timeSpan)
    {
        var format = timeSpan.Days > 0
            ? @"d\.hh\:mm\:ss"
            : @"hh\:mm\:ss";
        return timeSpan.ToString(format);
    }
}