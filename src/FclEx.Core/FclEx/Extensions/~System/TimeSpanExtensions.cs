namespace FclEx.Extensions;

public static class TimeSpanExtensions
{
    private const int TicksPerMicrosecond = 10;

    /// <summary>
    /// Multiplies a timespan by an integer value
    /// </summary>
    public static TimeSpan Multiply(this TimeSpan timeSpan, int factor)
    {
        return TimeSpan.FromTicks(checked(timeSpan.Ticks * factor));
    }

    /// <summary>
    /// Multiplies a timespan by a double value
    /// </summary>
    public static TimeSpan Multiply(this TimeSpan timeSpan, double factor)
    {
        if (double.IsNaN(factor))
            throw new ArgumentException("Factor cannot be NaN.", nameof(factor));

        var ticks = timeSpan.Ticks * factor;
        if (double.IsInfinity(ticks) || ticks > long.MaxValue || ticks < long.MinValue)
            throw new OverflowException();

        return TimeSpan.FromTicks((long)ticks);
    }

    /// <summary>
    /// Formats the <see cref="TimeSpan"/> into a string, strictly excluding the millisecond component.
    /// Unlike the default ToString(), this method ensures the precision stops at seconds.
    /// </summary>
    /// <param name="timeSpan">The time interval to format.</param>
    /// <returns>
    /// A string formatted as "hh:mm:ss" or "d.hh:mm:ss" depending on the duration.
    /// </returns>
    public static string ToSecondsString(this TimeSpan timeSpan)
    {
        var format = timeSpan.Days != 0
            ? @"d\.hh\:mm\:ss"
            : @"hh\:mm\:ss";
        var str = timeSpan.ToString(format);
        if (timeSpan < TimeSpan.Zero)
        {
            str = "-" + str;
        }
        return str;
    }

    public static TimeSpan With(this TimeSpan timeSpan,
        int? days = null,
        int? hours = null,
        int? minutes = null,
        int? seconds = null,
        int? milliseconds = null,
        int? microseconds = null,
        int? ticks = null)
    {
        return TimeSpan.New(
            days ?? timeSpan.Days,
            hours ?? timeSpan.Hours,
            minutes ?? timeSpan.Minutes,
            seconds ?? timeSpan.Seconds,
            milliseconds ?? timeSpan.Milliseconds,
            microseconds ?? GetMicrosecondComponent(timeSpan),
            ticks ?? GetTickComponent(timeSpan)
        );
    }

    /// <summary>
    /// Truncates the <see cref="TimeSpan"/> to millisecond precision by removing any sub-millisecond components.
    /// </summary>
    /// <param name="timeSpan">The <see cref="TimeSpan"/> to truncate.</param>
    /// <returns>
    /// A new <see cref="TimeSpan"/> whose value is truncated to milliseconds.
    /// </returns>
    /// <remarks>
    /// This is useful when working with systems or databases that do not support
    /// microsecond or tick-level precision and may otherwise round or reject values.
    /// </remarks>
    public static TimeSpan TruncateToMilliseconds(this TimeSpan timeSpan)
    {
        return timeSpan.TruncateTo(TimeSpan.TicksPerMillisecond);
    }

    public static TimeSpan TruncateToSeconds(this TimeSpan timeSpan)
    {
        return timeSpan.TruncateTo(TimeSpan.TicksPerSecond);
    }

    public static TimeSpan TruncateToMinutes(this TimeSpan timeSpan)
    {
        return timeSpan.TruncateTo(TimeSpan.TicksPerMinute);
    }

    public static TimeSpan TruncateToHours(this TimeSpan timeSpan)
    {
        return timeSpan.TruncateTo(TimeSpan.TicksPerHour);
    }

    public static TimeSpan TruncateToDays(this TimeSpan timeSpan)
    {
        return timeSpan.TruncateTo(TimeSpan.TicksPerDay);
    }

    public static long TotalWholeMilliseconds(this TimeSpan timeSpan)
    {
        return timeSpan.Ticks / TimeSpan.TicksPerMillisecond;
    }

    public static long TotalWholeSeconds(this TimeSpan timeSpan)
    {
        return timeSpan.Ticks / TimeSpan.TicksPerSecond;
    }

    public static long TotalWholeMinutes(this TimeSpan timeSpan)
    {
        return timeSpan.Ticks / TimeSpan.TicksPerMinute;
    }

    public static long TotalWholeHours(this TimeSpan timeSpan)
    {
        return timeSpan.Ticks / TimeSpan.TicksPerHour;
    }

    public static long TotalWholeDays(this TimeSpan timeSpan)
    {
        return timeSpan.Ticks / TimeSpan.TicksPerDay;
    }

    /// <summary>
    /// Converts the <see cref="TimeSpan"/> to a compact string such as "1d2h3m4s".
    /// </summary>
    public static string ToCompactString(this TimeSpan timeSpan)
    {
        if (timeSpan == TimeSpan.Zero)
            return "0s";

        var ticks = timeSpan.Ticks;
        var negative = ticks < 0;
        var absoluteTicks = GetAbsoluteTicks(ticks);

        var days = absoluteTicks / (ulong)TimeSpan.TicksPerDay;
        absoluteTicks %= (ulong)TimeSpan.TicksPerDay;
        var hours = absoluteTicks / (ulong)TimeSpan.TicksPerHour;
        absoluteTicks %= (ulong)TimeSpan.TicksPerHour;
        var minutes = absoluteTicks / (ulong)TimeSpan.TicksPerMinute;
        absoluteTicks %= (ulong)TimeSpan.TicksPerMinute;
        var seconds = absoluteTicks / (ulong)TimeSpan.TicksPerSecond;

        if (days == 0 && hours == 0 && minutes == 0 && seconds == 0)
            return "0s";

        using var disposable = StringBuilderHelper.GetCached();
        var builder = disposable.Value;
        if (negative)
        {
            builder.Append('-');
        }

        if (days != 0)
        {
            builder.Append(days).Append('d');
        }

        if (hours != 0)
        {
            builder.Append(hours).Append('h');
        }

        if (minutes != 0)
        {
            builder.Append(minutes).Append('m');
        }

        if (seconds != 0)
        {
            builder.Append(seconds).Append('s');
        }

        return builder.ToString();
    }

    private static TimeSpan TruncateTo(this TimeSpan timeSpan, long ticksPerUnit)
    {
        return TimeSpan.FromTicks(timeSpan.Ticks / ticksPerUnit * ticksPerUnit);
    }

    private static int GetMicrosecondComponent(TimeSpan timeSpan)
    {
        return (int)((timeSpan.Ticks % TimeSpan.TicksPerMillisecond) / TicksPerMicrosecond);
    }

    private static int GetTickComponent(TimeSpan timeSpan)
    {
        return (int)(timeSpan.Ticks % TicksPerMicrosecond);
    }

    private static ulong GetAbsoluteTicks(long ticks)
    {
        return ticks == long.MinValue
            ? (ulong)long.MaxValue + 1
            : (ulong)Math.Abs(ticks);
    }

    extension(TimeSpan)
    {
        public static int TicksPerMicrosecond => 10;

        public static TimeSpan New(int days, int hours, int minutes, int seconds, int milliseconds, int microseconds, int ticks = 0)
        {
            var totalTicks = checked(
                days * TimeSpan.TicksPerDay +
                hours * TimeSpan.TicksPerHour +
                minutes * TimeSpan.TicksPerMinute +
                seconds * TimeSpan.TicksPerSecond +
                milliseconds * TimeSpan.TicksPerMillisecond +
                microseconds * TicksPerMicrosecond +
                ticks);
            return TimeSpan.FromTicks(totalTicks);
        }

        public static TimeSpan operator *(TimeSpan timeSpan, int factor)
        {
            return timeSpan.Multiply(factor);
        }

        public static TimeSpan operator *(TimeSpan timeSpan, double factor)
        {
            return timeSpan.Multiply(factor);
        }
    }
}
