namespace FclEx.Utils;

public class SafeCounter
{
    private int _value;

    public SafeCounter(int value = 0)
    {
        _value = value;
    }

    public int Value => Volatile.Read(ref _value);

    /// <summary>
    /// Increments this counter, as an atomic operation.
    /// </summary>
    /// <returns>The incremented value.</returns>
    [MethodImpl(AggressiveInlining)]
    public int Increment() => Interlocked.Increment(ref _value);

    /// <summary>
    /// Decrements this counter, as an atomic operation.
    /// </summary>
    /// <returns>The decremented value.</returns>
    [MethodImpl(AggressiveInlining)]
    public int Decrement() => Interlocked.Decrement(ref _value);

    /// <summary>
    /// Adds a 32-bit integer to this counter, as an atomic operation.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>The new value.</returns>
    [MethodImpl(AggressiveInlining)]
    public int Add(int value) => Interlocked.Add(ref _value, value);

    /// <summary>
    /// Sets this counter to a specified value and returns the original value, as an atomic operation.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>The original value</returns>
    [MethodImpl(AggressiveInlining)]
    public int Set(int value) => Interlocked.Exchange(ref _value, value);

    /// <summary>
    /// Atomically increments the counter and resets it to zero when the increment reaches the specified threshold.
    /// </summary>
    /// <param name="threshold">The positive number of increments in one batch.</param>
    /// <returns>
    /// <see langword="true"/> only for the caller that completed and claimed a threshold-sized batch; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The increment and conditional reset are one compare-and-swap operation. Increments performed after a batch is
    /// claimed belong to the next batch and cannot be erased by the caller processing the completed batch.
    /// </remarks>
    public bool IncrementAndResetIfThresholdReached(int threshold)
    {
        if (threshold <= 0)
            throw new ArgumentOutOfRangeException(nameof(threshold), threshold, "Threshold must be greater than zero.");

        while (true)
        {
            var current = Volatile.Read(ref _value);
            var thresholdReached = current >= threshold - 1;
            var next = thresholdReached ? 0 : current + 1;
            if (Interlocked.CompareExchange(ref _value, next, current) == current)
                return thresholdReached;
        }
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
