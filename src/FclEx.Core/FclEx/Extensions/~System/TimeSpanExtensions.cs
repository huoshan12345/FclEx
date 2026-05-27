namespace FclEx.Extensions;

public static class TimeSpanExtensions
{
    private const int TicksPerMicrosecond = 10;

#if !NET5_0_OR_GREATER
    /// <summary>
    /// Multiplies a time interval by a floating-point factor.
    /// </summary>
    /// <param name="timeSpan">The time interval to multiply.</param>
    /// <param name="factor">The floating-point factor to multiply by.</param>
    /// <returns>A new <see cref="TimeSpan"/> whose ticks equal the rounded product of <paramref name="timeSpan"/> and <paramref name="factor"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="factor"/> is <see cref="double.NaN"/>.</exception>
    /// <exception cref="OverflowException">Thrown when the result is infinite, NaN, or outside the range of <see cref="TimeSpan"/>.</exception>
    public static TimeSpan Multiply(this TimeSpan timeSpan, double factor)
    {
        if (double.IsNaN(factor))
            throw new ArgumentException(SR.Overflow_TimeSpanTooLong);

        // Rounding to the nearest tick is as close to the result we would have with unlimited
        // precision as possible, and so likely to have the least potential to surprise.
        var ticks = Math.Round(timeSpan.Ticks * factor);
        return IntervalFromDoubleTicks(ticks);
    }

    /// <summary>
    /// Divides a time interval by a floating-point divisor.
    /// </summary>
    /// <param name="timeSpan">The time interval to divide.</param>
    /// <param name="divisor">The floating-point divisor to divide by.</param>
    /// <returns>A new <see cref="TimeSpan"/> whose ticks equal the rounded quotient of <paramref name="timeSpan"/> and <paramref name="divisor"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="divisor"/> is <see cref="double.NaN"/>.</exception>
    /// <exception cref="OverflowException">Thrown when the result is infinite, NaN, or outside the range of <see cref="TimeSpan"/>.</exception>
    public static TimeSpan Divide(this TimeSpan timeSpan, double divisor)
    {
        if (double.IsNaN(divisor))
            throw new ArgumentException(SR.Arg_CannotBeNaN, nameof(divisor));

        var ticks = Math.Round(timeSpan.Ticks / divisor);
        return IntervalFromDoubleTicks(ticks);
    }

    private static TimeSpan IntervalFromDoubleTicks(double ticks)
    {
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (ticks is > long.MaxValue or < long.MinValue or double.NaN)
            throw new OverflowException("TimeSpan overflowed because the duration is too long.");

        return new TimeSpan((long)ticks);
    }
#endif

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

    /// <summary>
    /// Creates a new <see cref="TimeSpan"/> by replacing selected component values.
    /// </summary>
    /// <param name="timeSpan">The time interval whose components are used as defaults.</param>
    /// <param name="days">The optional day component.</param>
    /// <param name="hours">The optional hour component.</param>
    /// <param name="minutes">The optional minute component.</param>
    /// <param name="seconds">The optional second component.</param>
    /// <param name="milliseconds">The optional millisecond component.</param>
    /// <param name="microseconds">The optional microsecond component.</param>
    /// <param name="ticks">The optional sub-microsecond tick component. One tick is 100 nanoseconds.</param>
    /// <returns>A new <see cref="TimeSpan"/> with the specified components replaced and all other components preserved.</returns>
    /// <exception cref="OverflowException">Thrown when the resulting component combination is outside the range of <see cref="TimeSpan"/>.</exception>
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

    /// <summary>
    /// Truncates the <see cref="TimeSpan"/> to second precision by removing sub-second components.
    /// </summary>
    /// <param name="timeSpan">The <see cref="TimeSpan"/> to truncate.</param>
    /// <returns>A new <see cref="TimeSpan"/> whose value is truncated to whole seconds.</returns>
    public static TimeSpan TruncateToSeconds(this TimeSpan timeSpan)
    {
        return timeSpan.TruncateTo(TimeSpan.TicksPerSecond);
    }

    /// <summary>
    /// Truncates the <see cref="TimeSpan"/> to minute precision by removing sub-minute components.
    /// </summary>
    /// <param name="timeSpan">The <see cref="TimeSpan"/> to truncate.</param>
    /// <returns>A new <see cref="TimeSpan"/> whose value is truncated to whole minutes.</returns>
    public static TimeSpan TruncateToMinutes(this TimeSpan timeSpan)
    {
        return timeSpan.TruncateTo(TimeSpan.TicksPerMinute);
    }

    /// <summary>
    /// Truncates the <see cref="TimeSpan"/> to hour precision by removing sub-hour components.
    /// </summary>
    /// <param name="timeSpan">The <see cref="TimeSpan"/> to truncate.</param>
    /// <returns>A new <see cref="TimeSpan"/> whose value is truncated to whole hours.</returns>
    public static TimeSpan TruncateToHours(this TimeSpan timeSpan)
    {
        return timeSpan.TruncateTo(TimeSpan.TicksPerHour);
    }

    /// <summary>
    /// Truncates the <see cref="TimeSpan"/> to day precision by removing sub-day components.
    /// </summary>
    /// <param name="timeSpan">The <see cref="TimeSpan"/> to truncate.</param>
    /// <returns>A new <see cref="TimeSpan"/> whose value is truncated to whole days.</returns>
    public static TimeSpan TruncateToDays(this TimeSpan timeSpan)
    {
        return timeSpan.TruncateTo(TimeSpan.TicksPerDay);
    }

    /// <summary>
    /// Gets the total number of complete milliseconds in the time interval.
    /// </summary>
    /// <param name="timeSpan">The time interval to inspect.</param>
    /// <returns>The whole millisecond count, truncated toward zero.</returns>
    public static long TotalWholeMilliseconds(this TimeSpan timeSpan)
    {
        return timeSpan.Ticks / TimeSpan.TicksPerMillisecond;
    }

    /// <summary>
    /// Gets the total number of complete seconds in the time interval.
    /// </summary>
    /// <param name="timeSpan">The time interval to inspect.</param>
    /// <returns>The whole second count, truncated toward zero.</returns>
    public static long TotalWholeSeconds(this TimeSpan timeSpan)
    {
        return timeSpan.Ticks / TimeSpan.TicksPerSecond;
    }

    /// <summary>
    /// Gets the total number of complete minutes in the time interval.
    /// </summary>
    /// <param name="timeSpan">The time interval to inspect.</param>
    /// <returns>The whole minute count, truncated toward zero.</returns>
    public static long TotalWholeMinutes(this TimeSpan timeSpan)
    {
        return timeSpan.Ticks / TimeSpan.TicksPerMinute;
    }

    /// <summary>
    /// Gets the total number of complete hours in the time interval.
    /// </summary>
    /// <param name="timeSpan">The time interval to inspect.</param>
    /// <returns>The whole hour count, truncated toward zero.</returns>
    public static long TotalWholeHours(this TimeSpan timeSpan)
    {
        return timeSpan.Ticks / TimeSpan.TicksPerHour;
    }

    /// <summary>
    /// Gets the total number of complete days in the time interval.
    /// </summary>
    /// <param name="timeSpan">The time interval to inspect.</param>
    /// <returns>The whole day count, truncated toward zero.</returns>
    public static long TotalWholeDays(this TimeSpan timeSpan)
    {
        return timeSpan.Ticks / TimeSpan.TicksPerDay;
    }

    /// <summary>
    /// Converts the <see cref="TimeSpan"/> to a compact string such as "1d2h3m4s".
    /// </summary>
    /// <param name="timeSpan">The time interval to format.</param>
    /// <returns>A compact day/hour/minute/second string, or <c>0s</c> when the absolute value is less than one second.</returns>
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
        /// <summary>
        /// Gets the number of ticks in one microsecond.
        /// </summary>
        public static int TicksPerMicrosecond => 10;

        /// <summary>
        /// Creates a <see cref="TimeSpan"/> from day through tick components.
        /// </summary>
        /// <param name="days">The day component.</param>
        /// <param name="hours">The hour component.</param>
        /// <param name="minutes">The minute component.</param>
        /// <param name="seconds">The second component.</param>
        /// <param name="milliseconds">The millisecond component.</param>
        /// <param name="microseconds">The microsecond component.</param>
        /// <param name="ticks">The sub-microsecond tick component. One tick is 100 nanoseconds.</param>
        /// <returns>A <see cref="TimeSpan"/> composed from the supplied components.</returns>
        /// <exception cref="OverflowException">Thrown when the resulting tick count is outside the range of <see cref="TimeSpan"/>.</exception>
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

#if !NET5_0_OR_GREATER
        /// <summary>
        /// Multiplies a time interval by a floating-point factor.
        /// </summary>
        /// <param name="timeSpan">The time interval to multiply.</param>
        /// <param name="factor">The floating-point factor to multiply by.</param>
        /// <returns>The multiplied time interval, rounded to the nearest tick.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="factor"/> is <see cref="double.NaN"/>.</exception>
        /// <exception cref="OverflowException">Thrown when the result is infinite, NaN, or outside the range of <see cref="TimeSpan"/>.</exception>
        public static TimeSpan operator *(TimeSpan timeSpan, double factor)
        {
            return timeSpan.Multiply(factor);
        }

        /// <summary>
        /// Multiplies a time interval by a floating-point factor.
        /// </summary>
        /// <param name="factor">The floating-point factor to multiply by.</param>
        /// <param name="timeSpan">The time interval to multiply.</param>
        /// <returns>The multiplied time interval, rounded to the nearest tick.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="factor"/> is <see cref="double.NaN"/>.</exception>
        /// <exception cref="OverflowException">Thrown when the result is infinite, NaN, or outside the range of <see cref="TimeSpan"/>.</exception>
        public static TimeSpan operator *(double factor, TimeSpan timeSpan)
        {
            return timeSpan.Multiply(factor);
        }

        /// <summary>
        /// Divides a time interval by a floating-point divisor.
        /// </summary>
        /// <param name="timeSpan">The time interval to divide.</param>
        /// <param name="divisor">The floating-point divisor to divide by.</param>
        /// <returns>The divided time interval, rounded to the nearest tick.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="divisor"/> is <see cref="double.NaN"/>.</exception>
        /// <exception cref="OverflowException">Thrown when the result is infinite, NaN, or outside the range of <see cref="TimeSpan"/>.</exception>
        public static TimeSpan operator /(TimeSpan timeSpan, double divisor)
        {
            return timeSpan.Divide(divisor);
        }
#endif
    }
}
