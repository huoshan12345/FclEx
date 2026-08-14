namespace FclEx.Utils;

/// <summary>
/// Lazily creates a value and refreshes it after the configured lifetime has elapsed.
/// </summary>
/// <typeparam name="T">The type of value.</typeparam>
public sealed class ExpiringLazy<T> : IDisposable
{
    private readonly Func<T> _factory;
    private readonly TimeSpan _lifetime;
    private readonly object _lock = new();

    private T? _value;
    private DateTime _expiresOn = DateTime.MinValue;
    private bool _hasValue;
    private bool _isCreating;
    private bool _isDisposed;

    public ExpiringLazy(Func<T> factory, TimeSpan lifetime)
    {
        _factory = Check.NotNull(factory);
        _lifetime = Check.GreaterThan(lifetime, TimeSpan.Zero);
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

                    if (_hasValue && _expiresOn >= DateTime.UtcNow)
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
        DateTime expiresOn;

        try
        {
            newValue = _factory();
        }
        catch
        {
            CompleteCreation();
            throw;
        }

        try
        {
            expiresOn = DateTime.UtcNow.Add(_lifetime);
        }
        catch
        {
            CompleteCreation();
            DisposeValue(newValue);
            throw;
        }

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
                _expiresOn = expiresOn;
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
        T? value = default;
        var hasValue = false;

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

        GC.SuppressFinalize(this);
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
