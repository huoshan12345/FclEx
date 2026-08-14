namespace FclEx.Utils;

/// <summary>
/// Provides thread-safe counters for a consumer's processing outcomes.
/// </summary>
public sealed class ConsumerMetrics
{
    private long _consumedItemCount;
    private long _failedConsumptionCount;
    private long _discardedItemCount;

    /// <summary>Gets the number of items successfully consumed.</summary>
    public long ConsumedItemCount => Interlocked.Read(ref _consumedItemCount);

    /// <summary>Gets the number of consumer delegate invocations that failed.</summary>
    public long FailedConsumptionCount => Interlocked.Read(ref _failedConsumptionCount);

    /// <summary>Gets the number of items discarded after exhausting their retries.</summary>
    public long DiscardedItemCount => Interlocked.Read(ref _discardedItemCount);

    internal void RecordConsumed(int count = 1) => Interlocked.Add(ref _consumedItemCount, count);

    internal void RecordFailure() => Interlocked.Increment(ref _failedConsumptionCount);

    internal void RecordDiscarded(int count = 1) => Interlocked.Add(ref _discardedItemCount, count);
}
