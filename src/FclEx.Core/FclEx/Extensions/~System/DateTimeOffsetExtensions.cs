namespace FclEx.Extensions;

public static class DateTimeOffsetExtensions
{
    private static readonly TimeSpan ChinaStandardTimeOffset = TimeSpan.FromHours(8);

    /// <summary>
    /// Converts an instant to China Standard Time (UTC+08:00) while preserving the represented instant.
    /// </summary>
    public static DateTimeOffset ToChinaStandardTime(this DateTimeOffset time)
    {
        return time.ToOffset(ChinaStandardTimeOffset);
    }

    public static TimeSpan Duration(this DateTimeOffset time, DateTimeOffset other)
    {
        return (time - other).Duration();
    }

    /// <summary>
    /// Truncates the <see cref="DateTimeOffset"/> to millisecond precision by removing any sub-millisecond components.
    /// </summary>
    /// <param name="time">The <see cref="DateTimeOffset"/> value to truncate.</param>
    /// <returns>
    /// A new <see cref="DateTimeOffset"/> truncated to milliseconds, preserving the original offset.
    /// </returns>
    /// <remarks>
    /// This is useful when interacting with systems or databases that do not support
    /// microsecond or tick-level precision and may otherwise round or reject values.
    /// Truncation is performed by discarding sub-millisecond precision; no rounding occurs.
    /// </remarks>
    public static DateTimeOffset TruncateToMilliseconds(this DateTimeOffset time)
    {
        return new DateTimeOffset(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second, time.Millisecond, time.Offset);
    }

    /// <summary>
    /// Truncates the <see cref="DateTimeOffset"/> to second precision by removing any sub-second components.
    /// </summary>
    /// <param name="time">The <see cref="DateTimeOffset"/> value to truncate.</param>
    /// <returns>
    /// A new <see cref="DateTimeOffset"/> truncated to seconds, preserving the original offset.
    /// </returns>
    public static DateTimeOffset TruncateToSeconds(this DateTimeOffset time)
    {
        return new DateTimeOffset(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second, 0, time.Offset);
    }
}
