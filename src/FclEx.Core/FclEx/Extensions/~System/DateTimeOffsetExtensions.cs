namespace FclEx.Extensions;

public static class DateTimeOffsetExtensions
{
    public static readonly TimeZoneInfo CnTimeZone = TimeZoneInfo.CreateCustomTimeZone("China", TimeSpan.FromHours(8), null, null);

    public static DateTimeOffset SetOffset(this DateTimeOffset time, TimeSpan offset)
    {
        return new DateTimeOffset(time.DateTime, offset);
    }

    public static DateTimeOffset SetCnOffset(this DateTimeOffset time)
    {
        return time.SetOffset(CnTimeZone.BaseUtcOffset);
    }

    public static DateTimeOffset ToCnTime(this DateTimeOffset time)
    {
        return time.ToOffset(CnTimeZone.BaseUtcOffset);
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
}
