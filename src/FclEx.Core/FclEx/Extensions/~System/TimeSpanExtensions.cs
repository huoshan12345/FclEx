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

    /// <summary>
    /// Truncates the <see cref="TimeSpan"/> to millisecond precision by removing any sub-millisecond components.
    /// </summary>
    /// <param name="time">The <see cref="TimeSpan"/> to truncate.</param>
    /// <returns>
    /// A new <see cref="TimeSpan"/> whose value is truncated to milliseconds.
    /// </returns>
    /// <remarks>
    /// This is useful when working with systems or databases that do not support
    /// microsecond or tick-level precision and may otherwise round or reject values.
    /// </remarks>
    public static TimeSpan TruncateToMilliseconds(this TimeSpan time)
    {
        return new TimeSpan(time.Days, time.Hours, time.Minutes, time.Seconds, time.Milliseconds);
    }
}