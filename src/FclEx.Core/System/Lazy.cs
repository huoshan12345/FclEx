namespace System;

/// <summary>
/// Provides factory methods for lazy value containers.
/// </summary>
public static class Lazy
{
    /// <summary>
    /// Creates a standard lazy value.
    /// </summary>
    /// <typeparam name="T">The type of value.</typeparam>
    /// <param name="valueFactory">The factory used to create the value.</param>
    /// <param name="isThreadSafe">Whether the returned lazy instance is safe for concurrent access.</param>
    /// <returns>A new lazy value.</returns>
    public static Lazy<T> Create<T>(Func<T> valueFactory, bool isThreadSafe = true)
    {
        return new Lazy<T>(valueFactory, isThreadSafe);
    }

    /// <summary>
    /// Creates a timer-based lazy value with an explicit initial delay and reset period.
    /// </summary>
    /// <typeparam name="T">The type of value.</typeparam>
    /// <param name="valueFactory">The factory used to create the value on demand.</param>
    /// <param name="dueTime">The delay before the first reset attempt.</param>
    /// <param name="period">The interval between subsequent reset attempts.</param>
    /// <param name="releaseValue">An optional callback that releases values removed from the container.</param>
    /// <returns>A new timer-based lazy value.</returns>
    public static TimerLazy<T> CreateTimerLazy<T>(Func<T> valueFactory, TimeSpan dueTime, TimeSpan period,
        Action<T>? releaseValue = null)
    {
        return new TimerLazy<T>(valueFactory, dueTime, period, releaseValue);
    }
}
