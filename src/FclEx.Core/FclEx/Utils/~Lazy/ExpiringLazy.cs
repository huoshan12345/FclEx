namespace FclEx.Utils;

/// <summary>
/// Lazily creates a value and refreshes it after the configured time-to-live has elapsed according to a monotonic clock.
/// </summary>
/// <typeparam name="T">The type of value created by the factory.</typeparam>
/// <remarks>
/// Expiration is measured from successful factory completion. Only one thread runs the factory at a time, and other
/// callers wait for that refresh. If a refresh fails, the expired value remains retained and the next access retries.
/// Recursive access to <see cref="Value"/> from the value factory is not supported.
/// </remarks>
public sealed class ExpiringLazy<T> : IDisposable
{
    private readonly Func<T> _valueFactory;
    private readonly Action<T>? _releaseValue;
    private readonly double _timeToLiveTimestampCount;
    private readonly object _lock = new();

    private T? _value;
    private long _createdAtTimestamp;
    private int _creatingThreadId;
    private bool _hasValue;
    private bool _isCreating;
    private bool _isDisposed;

    /// <summary>
    /// Initializes an expiring lazy value.
    /// </summary>
    /// <param name="valueFactory">The factory used to create or refresh the value.</param>
    /// <param name="timeToLive">The positive duration for which a successfully created value remains current.</param>
    /// <param name="releaseValue">
    /// An optional callback invoked outside the internal lock when a value is replaced or the instance is disposed.
    /// The callback is responsible for any required resource cleanup.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="valueFactory"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeToLive"/> is not positive.</exception>
    public ExpiringLazy(Func<T> valueFactory, TimeSpan timeToLive, Action<T>? releaseValue = null)
    {
        _valueFactory = Check.NotNull(valueFactory);
        _releaseValue = releaseValue;
        timeToLive = Check.GreaterThan(timeToLive, TimeSpan.Zero);
        _timeToLiveTimestampCount = timeToLive.TotalSeconds * Stopwatch.Frequency;
    }

    /// <summary>
    /// Gets the current value, creating or refreshing it when necessary.
    /// </summary>
    /// <exception cref="InvalidOperationException">The value factory recursively accesses this property.</exception>
    /// <exception cref="ObjectDisposedException">The instance has been disposed.</exception>
    public T Value
    {
        get
        {
            lock (_lock)
            {
                while (true)
                {
                    ThrowIfDisposed();

                    if (_hasValue && IsCurrent())
                        return _value!;

                    if (_isCreating == false)
                    {
                        _isCreating = true;
                        _creatingThreadId = Environment.CurrentManagedThreadId;
                        break;
                    }

                    if (_creatingThreadId == Environment.CurrentManagedThreadId)
                        throw new InvalidOperationException("The value factory cannot access Value recursively.");

                    Monitor.Wait(_lock);
                }
            }

            return CreateAndPublishValue();
        }
    }

    /// <summary>
    /// Gets whether a value has been created successfully, including one that is currently expired.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The instance has been disposed.</exception>
    public bool IsValueCreated
    {
        get
        {
            lock (_lock)
            {
                ThrowIfDisposed();
                return _hasValue;
            }
        }
    }

    private bool IsCurrent() => Stopwatch.GetTimestamp() - _createdAtTimestamp <= _timeToLiveTimestampCount;

    private T CreateAndPublishValue()
    {
        T newValue;
        try
        {
            newValue = _valueFactory();
        }
        catch
        {
            CompleteCreation();
            throw;
        }

        var createdAtTimestamp = Stopwatch.GetTimestamp();
        T? oldValue = default;
        var hasOldValue = false;
        var publishValue = false;

        lock (_lock)
        {
            if (_isDisposed == false)
            {
                oldValue = _value;
                hasOldValue = _hasValue;
                _value = newValue;
                _createdAtTimestamp = createdAtTimestamp;
                _hasValue = true;
                publishValue = true;
            }

            CompleteCreationWhileLocked();
        }

        if (publishValue == false)
        {
            ReleaseValue(newValue);
            throw new ObjectDisposedException(GetType().FullName);
        }

        if (hasOldValue && ReferenceEquals(oldValue, newValue) == false)
            ReleaseValue(oldValue!);

        return newValue;
    }

    private void CompleteCreation()
    {
        lock (_lock)
        {
            CompleteCreationWhileLocked();
        }
    }

    private void CompleteCreationWhileLocked()
    {
        _isCreating = false;
        _creatingThreadId = 0;
        Monitor.PulseAll(_lock);
    }

    private void ReleaseValue(T value) => _releaseValue?.Invoke(value);

    /// <summary>
    /// Disposes the lazy container and releases its currently retained value, if any.
    /// </summary>
    /// <remarks>
    /// An in-progress factory is not synchronously awaited. If it later completes, its result is released instead of
    /// being published.
    /// </remarks>
    public void Dispose()
    {
        T? valueToRelease;
        var hasValueToRelease = false;

        lock (_lock)
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            valueToRelease = _value;
            hasValueToRelease = _hasValue;
            _value = default;
            _hasValue = false;
            Monitor.PulseAll(_lock);
        }

        if (hasValueToRelease)
            ReleaseValue(valueToRelease!);
    }

    private void ThrowIfDisposed()
    {
        if (_isDisposed)
            throw new ObjectDisposedException(GetType().FullName);
    }
}
