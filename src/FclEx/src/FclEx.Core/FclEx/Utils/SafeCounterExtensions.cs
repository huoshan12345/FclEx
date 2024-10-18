namespace FclEx.Utils;

public static class SafeCounterExtensions
{
    /// <summary>
    /// Sets this counter to 0 and returns the original value, as an atomic operation.
    /// </summary>
    /// <returns>The original value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Reset(this SafeCounter counter) => counter.Set(0);

    public static async Task IncrementToThreshold(this SafeCounter counter, int threshold, Func<Task> action)
    {
        if (counter.Increment() >= threshold)
        {
            await action();
            counter.Reset();
        }
    }

    public static void IncrementToThreshold(this SafeCounter counter, int threshold, Action action)
    {
        if (counter.Increment() >= threshold)
        {
            action();
            counter.Reset();
        }
    }
}