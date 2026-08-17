namespace FclEx.Utils;

/// <summary>
/// Lazily creates a value and refreshes it after the configured lifetime has elapsed according to a monotonic clock.
/// </summary>
/// <typeparam name="T">The type of value.</typeparam>
public sealed class ExpiringLazy<T> : IDisposable
{
    private readonly Func<T> _factory;
    private readonly double _lifetimeTimestampCount;
    private readonly object _lock = new();

    private T? _value;
    private long _createdAtTimestamp;
    private bool _hasValue;
    private bool _isCreating;
    private bool _isDisposed;

    public ExpiringLazy(Func<T> factory, TimeSpan lifetime)
    {
        _factory = Check.NotNull(factory);
        lifetime = Check.GreaterThan(lifetime, TimeSpan.Zero);
        _lifetimeTimestampCount = lifetime.TotalSeconds * Stopwatch.Frequency;
    }

    public T Value
    {
        get
        {
            lock (_lock)
            {
                while (true)
                {
                    ThrowIfDisposed();

                    if (_hasValue && Stopwatch.GetTimestamp() - _createdAtTimestamp <= _lifetimeTimestampCount)
                        return _value!;

                    if (_isCreating == false)
                    {
                        _isCreating = true;
                        break;
                    }

                    Monitor.Wait(_lock);
                }
            }

            return CreateAndPublishValue();
        }
    }

    private T CreateAndPublishValue()
    {
        T newValue;
        long createdAtTimestamp;

        try
        {
            newValue = _factory();
        }
        catch
        {
            CompleteCreation();
            throw;
        }

        createdAtTimestamp = Stopwatch.GetTimestamp();

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

            _isCreating = false;
            Monitor.PulseAll(_lock);
        }

        if (publishValue == false)
        {
            DisposeValue(newValue);
            throw new ObjectDisposedException(GetType().FullName);
        }

        if (hasOldValue && ReferenceEquals(oldValue, newValue) == false)
            DisposeValue(oldValue!);

        return newValue;
    }

    private void CompleteCreation()
    {
        lock (_lock)
        {
            _isCreating = false;
            Monitor.PulseAll(_lock);
        }
    }

    public void Dispose()
    {
        T? value;
        bool hasValue;

        lock (_lock)
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            value = _value;
            hasValue = _hasValue;
            _value = default;
            _hasValue = false;
            Monitor.PulseAll(_lock);
        }

        if (hasValue)
            DisposeValue(value!);
    }

    private static void DisposeValue(T value)
    {
        if (value is IDisposable disposable)
            disposable.Dispose();
    }

    private void ThrowIfDisposed()
    {
        if (_isDisposed)
            throw new ObjectDisposedException(GetType().FullName);
    }
}
