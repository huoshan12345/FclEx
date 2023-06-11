namespace FclEx.Utils;

public class SafeCounter
{
    private volatile int _value;

    public SafeCounter(int value = 0)
    {
        _value = value;
    }

    public int Value => _value;

    /// <summary>
    /// Increments this counter, as an atomic operation.
    /// </summary>
    /// <returns>The incremented value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Increment() => Interlocked.Increment(ref _value);

    /// <summary>
    /// Decrements this counter, as an atomic operation.
    /// </summary>
    /// <returns>The decremented value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Decrement() => Interlocked.Decrement(ref _value);

    /// <summary>
    /// Adds a 32-bit integer to this counter, as an atomic operation.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>The new value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Add(int value) => Interlocked.Add(ref _value, value);

    /// <summary>
    /// Sets this counter to a specified value and returns the original value, as an atomic operation.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>The original value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Set(int value) => Interlocked.Exchange(ref _value, value);
}