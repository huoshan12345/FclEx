namespace FclEx.Extensions;

public static class TimeSpanExtensions
{
    /// <summary>
    /// Multiplies a timespan by an integer value
    /// </summary>
    public static TimeSpan Multiply(this TimeSpan timeSpan, int factor)
    {
        return TimeSpan.FromTicks(timeSpan.Ticks * factor);
    }

    /// <summary>
    /// Multiplies a timespan by a double value
    /// </summary>
    public static TimeSpan Multiply(this TimeSpan timeSpan, double factor)
    {
        return TimeSpan.FromTicks((long)(timeSpan.Ticks * factor));
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
        int? microseconds = null)
    {
        return TimeSpan.New(
            days ?? timeSpan.Days,
            hours ?? timeSpan.Hours,
            minutes ?? timeSpan.Minutes,
            seconds ?? timeSpan.Seconds,
            milliseconds ?? timeSpan.Milliseconds,
            microseconds ??
#if NET5_0_OR_GREATER
            timeSpan.Microseconds
#else
            (int)((timeSpan.Ticks % TimeSpan.TicksPerMillisecond) / 10)
#endif
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
        return timeSpan.With(microseconds: 0);
    }

    public static TimeSpan TruncateToSecond(this TimeSpan timeSpan)
    {
        return timeSpan.With(milliseconds: 0, microseconds: 0);
    }

    public static TimeSpan TruncateToMinute(this TimeSpan timeSpan)
    {
        return timeSpan.With(seconds: 0, milliseconds: 0, microseconds: 0);
    }

    public static TimeSpan TruncateToHour(this TimeSpan timeSpan)
    {
        return timeSpan.With(minutes: 0, seconds: 0, milliseconds: 0, microseconds: 0);
    }

    public static TimeSpan TruncateToDay(this TimeSpan timeSpan)
    {
        return timeSpan.With(hours: 0, minutes: 0, seconds: 0, milliseconds: 0, microseconds: 0);
    }

    public static long WholeMilliseconds(this TimeSpan timeSpan)
    {
        return timeSpan.Ticks / TimeSpan.TicksPerMillisecond;
    }

    public static long WholeSeconds(this TimeSpan timeSpan)
    {
        return timeSpan.Ticks / TimeSpan.TicksPerSecond;
    }

    public static long WholeMinutes(this TimeSpan timeSpan)
    {
        return timeSpan.Ticks / TimeSpan.TicksPerMinute;
    }

    public static long WholeHours(this TimeSpan timeSpan)
    {
        return timeSpan.Ticks / TimeSpan.TicksPerHour;
    }

    public static long WholeDays(this TimeSpan timeSpan)
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

        var disposable = StringBuilderHelper.GetCached();
        var builder = disposable.Value;

        if (timeSpan.Days != 0)
        {
            builder.Append(timeSpan.Days).Append('d');
        }

        if (timeSpan.Hours != 0)
        {
            builder.Append(timeSpan.Hours).Append('h');
        }

        if (timeSpan.Minutes != 0)
        {
            builder.Append(timeSpan.Minutes).Append('m');
        }

        if (timeSpan.Seconds != 0)
        {
            builder.Append(timeSpan.Seconds).Append('s');
        }

        return builder.ToString();
    }

    extension(TimeSpan)
    {
        public static TimeSpan New(int days, int hours, int minutes, int seconds, int milliseconds, int microseconds)
        {
#if NET5_0_OR_GREATER
            return new TimeSpan(days, hours, minutes, seconds, milliseconds, microseconds);
#else
            var ticks = days * TimeSpan.TicksPerDay +
                        hours * TimeSpan.TicksPerHour +
                        minutes * TimeSpan.TicksPerMinute +
                        seconds * TimeSpan.TicksPerSecond +
                        milliseconds * TimeSpan.TicksPerMillisecond +
                        microseconds * 10; // 1 microsecond = 10 ticks
            return new TimeSpan(ticks);
#endif
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