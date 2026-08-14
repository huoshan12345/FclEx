namespace FclEx.Utils;

public static class SafeCounterExtensions
{
    /// <summary>
    /// Sets this counter to 0 and returns the original value, as an atomic operation.
    /// </summary>
    /// <returns>The original value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Reset(this SafeCounter counter) => counter.Set(0);

    /// <summary>
    /// Atomically increments the counter and asynchronously invokes an action when this caller completes a batch.
    /// </summary>
    /// <remarks>
    /// The batch is claimed before the action starts. A failed action is not retried and does not restore the batch.
    /// Actions belonging to different completed batches may overlap.
    /// </remarks>
    public static async Task IncrementAndInvokeAtThresholdAsync(this SafeCounter counter, int threshold, Func<Task> action)
    {
        Check.NotNull(counter);
        Check.NotNull(action);

        if (counter.IncrementAndResetIfThresholdReached(threshold))
            await action();
    }

    /// <summary>Atomically increments the counter and invokes an action when this caller completes a batch.</summary>
    /// <remarks>
    /// The batch is claimed before the action starts. A failed action is not retried and does not restore the batch.
    /// Actions belonging to different completed batches may overlap.
    /// </remarks>
    public static void IncrementAndInvokeAtThreshold(this SafeCounter counter, int threshold, Action action)
    {
        Check.NotNull(counter);
        Check.NotNull(action);

        if (counter.IncrementAndResetIfThresholdReached(threshold))
            action();
    }
}
