namespace FclEx.Extensions;

public static class DateTimeExtensions
{
    public static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public const string ShortTimeFormat = "yyyyMMddHHmmss";
    public const string CommonTimeFormat = "yyyy-MM-dd HH:mm:ss";

    public static DateTimeOffset ToDateTimeOffset(this DateTime dateTime)
    {
        return new DateTimeOffset(dateTime);
    }

    /// <summary>
    /// Returns the number of seconds that have elapsed since 1970-01-01T00:00:00Z.
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static long ToUnixTimeSeconds(this DateTime dateTime)
    {
        return dateTime.ToDateTimeOffset().ToUnixTimeSeconds();
    }

    /// <summary>
    /// Returns the number of milliseconds that have elapsed since 1970-01-01T00:00:00.000Z.
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static long ToUnixTimeMilliseconds(this DateTime dateTime)
    {
        return dateTime.ToDateTimeOffset().ToUnixTimeMilliseconds();
    }

    public static string ToShort(this DateTime @this) => @this.ToString(ShortTimeFormat);

    public static string ToCommon(this DateTime @this) => @this.ToString(CommonTimeFormat);

    public static DateTime AddWeek(this DateTime dateTime) => dateTime.AddWeeks(1);

    public static DateTime AddWeeks(this DateTime dateTime, int numberOfWeeks)
    {
        return dateTime.AddDays(numberOfWeeks * 7);
    }

    public static DateTime StartOfWeek(this DateTime dt, int hour = 0, int minute = 0, int second = 0, DayOfWeek startOfWeek = DayOfWeek.Monday)
    {
        var diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
        return dt.Today(hour, minute, second).AddDays(-1 * diff);
    }

    public static DateTime EndOfWeek(this DateTime dt, int hour = 0, int minute = 0, int second = 0, DayOfWeek startOfWeek = DayOfWeek.Monday)
    {
        return dt.StartOfWeek(hour, minute, second, startOfWeek).AddDays(6);
    }

    public static DateTime Today(this DateTime dt, int hour = 0, int minute = 0, int second = 0)
    {
        return new DateTime(dt.Year, dt.Month, dt.Day, hour, minute, second);
    }

    public static DateTime Tomorrow(this DateTime dt, int hour = 0, int minute = 0, int second = 0)
    {
        return dt.Today(hour, minute, second).AddDays(1);
    }

    public static DateTime Yesterday(this DateTime dt, int hour = 0, int minute = 0, int second = 0)
    {
        return dt.Today(hour, minute, second).AddDays(-1);
    }

    public static DateTime ThisYear(this DateTime dt, int month, int day, int hour = 0, int minute = 0, int second = 0)
    {
        return new DateTime(dt.Year, month, day, hour, minute, second);
    }

    public static DateTime ThisMonth(this DateTime dt, int day, int hour = 0, int minute = 0, int second = 0)
    {
        return new DateTime(dt.Year, dt.Month, day, hour, minute, second);
    }

    public static DateTime EndOfMonth(this DateTime dt, int hour = 0, int minute = 0, int second = 0)
    {
        return new DateTime(dt.Year, dt.Month, DateTime.DaysInMonth(dt.Year, dt.Month), hour, minute, second);
    }

    public static DateTime StartOfMonth(this DateTime dt, int hour = 0, int minute = 0, int second = 0)
    {
        return new DateTime(dt.Year, dt.Month, 1, hour, minute, second);
    }

    public static DateTime LastTickOfMonth(this DateTime dt, int hour = 0, int minute = 0, int second = 0)
    {
        var lastDay = dt.EndOfMonth();
        return lastDay.AddDays(1).AddTicks(-1);
    }

    public static DateTime GetMaxTimeOfDate(this DateTime dt)
    {
        return dt.Date.AddDays(1).AddTicks(-1);
    }

    public static DateTime? GetMaxTimeOfDate(this DateTime? dt)
    {
        return dt?.GetMaxTimeOfDate();
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

    public static DateTime ToUtc(this DateTime dateTime)
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

    public static DateTime ToCnTime(this DateTime time)
    {
        return time.ToUtc().AddHours(8);
    }

    public static string ToCnTimeStr(this DateTime time)
    {
        return time.ToCnTime().ToCommon();
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