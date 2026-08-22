namespace FclEx.Utils;

/// <summary>
/// Lazily creates a value and periodically resets an already-created or faulted generation.
/// </summary>
/// <typeparam name="T">The type of value created by the factory.</typeparam>
/// <remarks>
/// The schedule starts when this instance is constructed. Timer callbacks do not create a value; they only invalidate
/// a completed generation so the next <see cref="Value"/> access creates one. Background reset failures are available
/// through <see cref="LastResetException"/>.
/// </remarks>
public sealed class TimerLazy<T> : IDisposable
{
    private readonly ResettableLazy<T> _lazy;
    private readonly Timer _timer;
    private readonly
#if NET9_0_OR_GREATER
        Lock
#else
        object
#endif
    _lock = new();

    private Exception? _lastResetException;
    private bool _isDisposed;

    /// <summary>
    /// Initializes a timer-based lazy value with an explicit initial delay and reset period.
    /// </summary>
    /// <param name="valueFactory">The factory used to create the value on demand.</param>
    /// <param name="dueTime">The delay before the first reset attempt.</param>
    /// <param name="period">The interval between subsequent reset attempts.</param>
    /// <param name="releaseValue">
    /// An optional callback invoked when a created value is reset, replaced, or disposed.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="valueFactory"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="dueTime"/> or <paramref name="period"/> is outside the range supported by the underlying timer.
    /// </exception>
    public TimerLazy(Func<T> valueFactory, TimeSpan dueTime, TimeSpan period, Action<T>? releaseValue = null)
    {
        _lazy = new ResettableLazy<T>(valueFactory, releaseValue);
        _timer = NonCapturingTimer.Create(OnTimer, dueTime, period);
    }

    /// <summary>
    /// Initializes a timer-based lazy value whose first and subsequent reset attempts occur after
    /// <paramref name="period"/>.
    /// </summary>
    /// <param name="valueFactory">The factory used to create the value on demand.</param>
    /// <param name="period">The positive interval between reset attempts.</param>
    /// <param name="releaseValue">
    /// An optional callback invoked when a created value is reset, replaced, or disposed.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="valueFactory"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="period"/> is not positive.</exception>
    public TimerLazy(Func<T> valueFactory, TimeSpan period, Action<T>? releaseValue = null)
        : this(valueFactory, ValidatePeriod(period), period, releaseValue)
    {
    }

    /// <summary>
    /// Gets the cached value, creating it on demand when necessary.
    /// </summary>
    /// <exception cref="InvalidOperationException">The value factory recursively accesses this property.</exception>
    /// <exception cref="ObjectDisposedException">The instance has been disposed.</exception>
    public T Value => _lazy.Value;

    /// <summary>
    /// Gets whether the current generation has successfully created a value.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The instance has been disposed.</exception>
    public bool IsValueCreated => _lazy.IsValueCreated;

    /// <summary>
    /// Gets the most recent exception thrown while releasing a value from a timer callback, or
    /// <see langword="null"/> if the latest reset succeeded.
    /// </summary>
    public Exception? LastResetException
    {
        get
        {
            lock (_lock)
            {
                return _lastResetException;
            }
        }
    }

    /// <summary>
    /// Invalidates the current generation so the next access creates a new value.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The instance has been disposed.</exception>
    public void Reset()
    {
        _lazy.Reset();
        ClearLastResetException();
    }

    /// <summary>
    /// Replaces the value factory and invalidates the current generation.
    /// </summary>
    /// <param name="valueFactory">The factory to use for subsequent value creation.</param>
    /// <exception cref="ArgumentNullException"><paramref name="valueFactory"/> is <see langword="null"/>.</exception>
    /// <exception cref="ObjectDisposedException">The instance has been disposed.</exception>
    public void ReplaceValueFactory(Func<T> valueFactory)
    {
        _lazy.ReplaceValueFactory(valueFactory);
        ClearLastResetException();
    }

    private void OnTimer()
    {
        lock (_lock)
        {
            if (_isDisposed)
                return;

            try
            {
                if (_lazy.TryReset())
                    _lastResetException = null;
            }
            catch (Exception ex)
            {
                _lastResetException = ex;
            }
        }
    }

    private void ClearLastResetException()
    {
        lock (_lock)
        {
            _lastResetException = null;
        }
    }

    /// <summary>
    /// Stops future timer callbacks, waits for an active reset callback, and disposes the underlying lazy value.
    /// </summary>
    public void Dispose()
    {
        _timer.Dispose();

        lock (_lock)
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            _lazy.Dispose();
        }
    }

    private static TimeSpan ValidatePeriod(TimeSpan period) => Check.GreaterThan(period, TimeSpan.Zero);
}
