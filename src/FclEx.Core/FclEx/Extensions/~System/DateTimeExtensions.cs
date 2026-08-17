namespace FclEx.Extensions;

public static class DateTimeExtensions
{
    public static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public const string ShortTimeFormat = "yyyyMMddHHmmss";
    public const string CommonTimeFormat = "yyyy-MM-dd HH:mm:ss";

    /// <summary>
    /// Converts a <see cref="DateTime"/> to a <see cref="DateTimeOffset"/>.
    /// </summary>
    /// <param name="dateTime">The date and time to convert.</param>
    /// <param name="offset">
    /// The explicit UTC offset to use, or <see langword="null"/> to use the same behavior as
    /// <see cref="DateTimeOffset.DateTimeOffset(DateTime)"/>.
    /// </param>
    /// <remarks>
    /// When <paramref name="offset"/> is supplied, the validation rules of
    /// <see cref="DateTimeOffset.DateTimeOffset(DateTime, TimeSpan)"/> apply, including its restrictions for
    /// <see cref="DateTimeKind.Utc"/> and <see cref="DateTimeKind.Local"/> values.
    /// </remarks>
    public static DateTimeOffset ToDateTimeOffset(this DateTime dateTime, TimeSpan? offset = null)
    {
        return offset is { } value
            ? new DateTimeOffset(dateTime, value)
            : new DateTimeOffset(dateTime);
    }

    /// <summary>
    /// Returns the number of seconds that have elapsed since 1970-01-01T00:00:00Z.
    /// </summary>
    /// <param name="dateTime">The date and time to convert.</param>
    /// <param name="offset">The explicit UTC offset, or <see langword="null"/> to use the <paramref name="dateTime"/> kind.</param>
    /// <returns>The Unix timestamp in seconds.</returns>
    public static long ToUnixTimeSeconds(this DateTime dateTime, TimeSpan? offset = null)
    {
        return dateTime.ToDateTimeOffset(offset).ToUnixTimeSeconds();
    }

    /// <summary>
    /// Returns the number of milliseconds that have elapsed since 1970-01-01T00:00:00.000Z.
    /// </summary>
    /// <param name="dateTime">The date and time to convert.</param>
    /// <param name="offset">The explicit UTC offset, or <see langword="null"/> to use the <paramref name="dateTime"/> kind.</param>
    /// <returns>The Unix timestamp in milliseconds.</returns>
    public static long ToUnixTimeMilliseconds(this DateTime dateTime, TimeSpan? offset = null)
    {
        return dateTime.ToDateTimeOffset(offset).ToUnixTimeMilliseconds();
    }

    public static string ToShort(this DateTime @this) => @this.ToString(ShortTimeFormat);

    public static string ToCommon(this DateTime @this) => @this.ToString(CommonTimeFormat);

    public static DateTime AddWeek(this DateTime dateTime) => dateTime.AddWeeks(1);

    public static DateTime AddWeeks(this DateTime dateTime, int numberOfWeeks)
    {
        return dateTime.AddDays(numberOfWeeks * 7);
    }

    public static DateTime FirstDayOfWeek(
        this DateTime dt,
        int hour = 0,
        int minute = 0,
        int second = 0,
        int millisecond = 0,
        DayOfWeek weekStartsOn = DayOfWeek.Monday)
    {
        if ((uint)weekStartsOn > (uint)DayOfWeek.Saturday)
            throw new ArgumentOutOfRangeException(nameof(weekStartsOn), weekStartsOn, null);

        var daysSinceStartOfWeek = (7 + (dt.DayOfWeek - weekStartsOn)) % 7;
        return dt.Today(hour, minute, second, millisecond).AddDays(-daysSinceStartOfWeek);
    }

    public static DateTime LastDayOfWeek(
        this DateTime dt,
        int hour = 0,
        int minute = 0,
        int second = 0,
        int millisecond = 0,
        DayOfWeek weekStartsOn = DayOfWeek.Monday)
    {
        return dt.FirstDayOfWeek(hour, minute, second, millisecond, weekStartsOn).AddDays(6);
    }

    public static DateTime Today(this DateTime dt, int hour = 0, int minute = 0, int second = 0, int millisecond = 0)
    {
        return new DateTime(dt.Year, dt.Month, dt.Day, hour, minute, second, millisecond, dt.Kind);
    }

    public static DateTime Tomorrow(this DateTime dt, int hour = 0, int minute = 0, int second = 0, int millisecond = 0)
    {
        return dt.Today(hour, minute, second, millisecond).AddDays(1);
    }

    public static DateTime Yesterday(this DateTime dt, int hour = 0, int minute = 0, int second = 0, int millisecond = 0)
    {
        return dt.Today(hour, minute, second, millisecond).AddDays(-1);
    }

    public static DateTime ThisYear(
        this DateTime dt,
        int month,
        int day,
        int hour = 0,
        int minute = 0,
        int second = 0,
        int millisecond = 0)
    {
        return new DateTime(dt.Year, month, day, hour, minute, second, millisecond, dt.Kind);
    }

    public static DateTime ThisMonth(
        this DateTime dt,
        int day,
        int hour = 0,
        int minute = 0,
        int second = 0,
        int millisecond = 0)
    {
        return new DateTime(dt.Year, dt.Month, day, hour, minute, second, millisecond, dt.Kind);
    }

    public static DateTime LastDayOfMonth(
        this DateTime dt,
        int hour = 0,
        int minute = 0,
        int second = 0,
        int millisecond = 0)
    {
        return new DateTime(dt.Year, dt.Month, DateTime.DaysInMonth(dt.Year, dt.Month), hour, minute, second, millisecond, dt.Kind);
    }

    public static DateTime FirstDayOfMonth(
        this DateTime dt,
        int hour = 0,
        int minute = 0,
        int second = 0,
        int millisecond = 0)
    {
        return new DateTime(dt.Year, dt.Month, 1, hour, minute, second, millisecond, dt.Kind);
    }

    public static DateTime LastTickOfDay(this DateTime dt)
    {
        return dt.Today().AddTicks(TimeSpan.TicksPerDay - 1);
    }

    public static DateTime LastTickOfWeek(this DateTime dt, DayOfWeek weekStartsOn = DayOfWeek.Monday)
    {
        return dt.LastDayOfWeek(weekStartsOn: weekStartsOn).AddTicks(TimeSpan.TicksPerDay - 1);
    }

    public static DateTime LastTickOfMonth(this DateTime dt)
    {
        return dt.LastDayOfMonth().AddTicks(TimeSpan.TicksPerDay - 1);
    }

    public static DateTime? LastTickOfDay(this DateTime? dt)
    {
        return dt?.LastTickOfDay();
    }

    public static DateTime? GetDate(this DateTime? dt)
    {
        return dt?.Date;
    }

    public static string ToStringOrEmpty(this DateTime? dateTime, string format = CommonTimeFormat)
    {
        return dateTime is { } dt
            ? dt.ToString(format)
            : string.Empty;
    }

    /// <summary>
    /// Converts a local value to UTC and treats an unspecified value as already being UTC.
    /// </summary>
    /// <remarks>
    /// For an <see cref="DateTimeKind.Unspecified"/> value, this method preserves the clock time and only changes its
    /// <see cref="DateTime.Kind"/> to <see cref="DateTimeKind.Utc"/>. It does not infer or apply a time zone.
    /// </remarks>
    public static DateTime AssumeUtc(this DateTime dateTime)
    {
        return dateTime.Kind switch
        {
            DateTimeKind.Local => dateTime.ToUniversalTime(),
            DateTimeKind.Unspecified => dateTime.SpecifyKind(DateTimeKind.Utc),
            DateTimeKind.Utc => dateTime,
            _ => throw new ArgumentOutOfRangeException(nameof(dateTime))
        };
    }

    public static DateTime SpecifyKind(this DateTime time, DateTimeKind kind)
    {
        return DateTime.SpecifyKind(time, kind);
    }

    /// <summary>
    /// Truncates the <see cref="DateTime"/> to millisecond precision by removing any sub-millisecond components.
    /// </summary>
    /// <param name="time">The <see cref="DateTime"/> value to truncate.</param>
    /// <returns>
    /// A new <see cref="DateTime"/> truncated to milliseconds, preserving the original <see cref="DateTimeKind"/>.
    /// </returns>
    /// <remarks>
    /// This is useful when interacting with systems or databases that do not support
    /// microsecond or tick-level precision and may otherwise round or reject values.
    /// Truncation is performed by discarding sub-millisecond precision; no rounding occurs.
    /// </remarks>
    public static DateTime TruncateToMilliseconds(this DateTime time)
    {
        return new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second, time.Millisecond, time.Kind);
    }
}
