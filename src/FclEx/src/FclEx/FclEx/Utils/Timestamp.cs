using System;
using System.Diagnostics;

namespace FclEx.Utils;

public struct Timestamp
{
    private static readonly double _timestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;

    public static long GetTimestamp() => Stopwatch.GetTimestamp();

    public static TimeSpan GetTimeSpan(long startTimestamp, long endTimestamp)
    {
        var timestampDelta = endTimestamp - startTimestamp;
        var ticks = (long)(_timestampToTicks * timestampDelta);
        return new TimeSpan(ticks);
    }

    public static TimeSpan GetElapsedTime(long startTimestamp)
    {
        var end = Stopwatch.GetTimestamp();
        return GetTimeSpan(startTimestamp, end);
    }
}