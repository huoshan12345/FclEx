namespace FclEx.Utils;

/// <summary>
/// Provides thread-safe lazy initialization whose value factory and cached state can be reset.
/// </summary>
/// <typeparam name="T">The type of value created by the factory.</typeparam>
/// <remarks>
/// Factory exceptions are cached until <see cref="Reset"/> or <see cref="ReplaceValueFactory"/> is called.
/// When a reset races with value creation, the obsolete result is not published and the accessing thread retries with
/// the current factory; a successfully produced obsolete value is released. Recursive access to <see cref="Value"/>
/// from the value factory is not supported.
/// </remarks>
public sealed class ResettableLazy<T> : IDisposable
{
    private readonly object _lock = new();
    private readonly Action<T>? _releaseValue;

    private Func<T> _valueFactory;
    private T? _value;
    private ExceptionDispatchInfo? _factoryException;
    private long _generation;
    private int _creatingThreadId;
    private bool _hasValue;
    private bool _isCreating;
    private bool _isDisposed;

    /// <summary>
    /// Initializes a resettable lazy value.
    /// </summary>
    /// <param name="valueFactory">The factory used to create the value on demand.</param>
    /// <param name="releaseValue">
    /// An optional callback invoked outside the internal lock when a created value is reset, replaced, or disposed.
    /// The callback is responsible for any required resource cleanup.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="valueFactory"/> is <see langword="null"/>.</exception>
    public ResettableLazy(Func<T> valueFactory, Action<T>? releaseValue = null)
    {
        _valueFactory = Check.NotNull(valueFactory);
        _releaseValue = releaseValue;
    }

    /// <summary>
    /// Gets the cached value, creating it once with the current factory when necessary.
    /// </summary>
    /// <exception cref="InvalidOperationException">The value factory recursively accesses this property.</exception>
    /// <exception cref="ObjectDisposedException">The instance has been disposed.</exception>
    public T Value
    {
        get
        {
            while (true)
            {
                Func<T>? valueFactory = null;
                ExceptionDispatchInfo? factoryException = null;
                long generation = 0;

                lock (_lock)
                {
                    while (valueFactory is null && factoryException is null)
                    {
                        ThrowIfDisposed();

                        if (_hasValue)
                            return _value!;

                        if (_factoryException is not null)
                        {
                            factoryException = _factoryException;
                            break;
                        }

                        if (_isCreating == false)
                        {
                            _isCreating = true;
                            _creatingThreadId = Environment.CurrentManagedThreadId;
                            valueFactory = _valueFactory;
                            generation = _generation;
                            break;
                        }

                        if (_creatingThreadId == Environment.CurrentManagedThreadId)
                            throw new InvalidOperationException("The value factory cannot access Value recursively.");

                        Monitor.Wait(_lock);
                    }
                }

                if (factoryException is not null)
                    return RethrowFactoryException(factoryException);

                T newValue;
                try
                {
                    newValue = valueFactory!();
                }
                catch (Exception ex)
                {
                    var failureResult = CompleteFailedCreation(generation, ex);
                    if (failureResult == FailureResult.Obsolete)
                        continue;

                    if (failureResult == FailureResult.Disposed)
                        throw new ObjectDisposedException(GetType().FullName);

                    throw;
                }

                var publishResult = TryPublishValue(generation, newValue);
                if (publishResult == PublishResult.Published)
                    return newValue;

                ReleaseValue(newValue);

                if (publishResult == PublishResult.Disposed)
                    throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }

    /// <summary>
    /// Gets whether the current generation has successfully created a value.
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

    /// <summary>
    /// Invalidates the current generation so the next access creates a new value.
    /// </summary>
    /// <remarks>
    /// A cached factory exception is cleared. If creation is currently in progress, its result is treated as obsolete
    /// and released instead of being published.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">The instance has been disposed.</exception>
    public void Reset()
    {
        var result = ResetCore(onlyIfCompleted: false, throwIfDisposed: true);
        if (result.HasValue)
            ReleaseValue(result.Value!);
    }

    /// <summary>
    /// Replaces the value factory and invalidates the current generation.
    /// </summary>
    /// <param name="valueFactory">The factory to use for subsequent value creation.</param>
    /// <exception cref="ArgumentNullException"><paramref name="valueFactory"/> is <see langword="null"/>.</exception>
    /// <exception cref="ObjectDisposedException">The instance has been disposed.</exception>
    public void ReplaceValueFactory(Func<T> valueFactory)
    {
        Check.NotNull(valueFactory);

        T? valueToRelease;
        var hasValueToRelease = false;
        lock (_lock)
        {
            ThrowIfDisposed();

            valueToRelease = _value;
            hasValueToRelease = _hasValue;
            _valueFactory = valueFactory;
            InvalidateCurrentGeneration();
        }

        if (hasValueToRelease)
            ReleaseValue(valueToRelease!);
    }

    internal bool TryReset()
    {
        var result = ResetCore(onlyIfCompleted: true, throwIfDisposed: false);
        if (result.WasReset == false)
            return false;

        if (result.HasValue)
            ReleaseValue(result.Value!);

        return true;
    }

    private ResetResult ResetCore(bool onlyIfCompleted, bool throwIfDisposed)
    {
        lock (_lock)
        {
            if (_isDisposed)
            {
                if (throwIfDisposed)
                    ThrowIfDisposed();

                return default;
            }

            if (onlyIfCompleted && _hasValue == false && _factoryException is null)
                return default;

            var result = new ResetResult(WasReset: true, _hasValue, _value);
            InvalidateCurrentGeneration();
            return result;
        }
    }

    private void InvalidateCurrentGeneration()
    {
        _generation++;
        _value = default;
        _hasValue = false;
        _factoryException = null;
        Monitor.PulseAll(_lock);
    }

    private FailureResult CompleteFailedCreation(long generation, Exception exception)
    {
        lock (_lock)
        {
            var result = _isDisposed
                ? FailureResult.Disposed
                : generation != _generation
                    ? FailureResult.Obsolete
                    : FailureResult.Current;

            if (result == FailureResult.Current)
                _factoryException = ExceptionDispatchInfo.Capture(exception);

            CompleteCreation();
            return result;
        }
    }

    private PublishResult TryPublishValue(long generation, T value)
    {
        lock (_lock)
        {
            var result = _isDisposed
                ? PublishResult.Disposed
                : generation != _generation
                    ? PublishResult.Obsolete
                    : PublishResult.Published;

            if (result == PublishResult.Published)
            {
                _value = value;
                _hasValue = true;
            }

            CompleteCreation();
            return result;
        }
    }

    private void CompleteCreation()
    {
        _isCreating = false;
        _creatingThreadId = 0;
        Monitor.PulseAll(_lock);
    }

    private void ReleaseValue(T value) => _releaseValue?.Invoke(value);

    private static T RethrowFactoryException(ExceptionDispatchInfo exception)
    {
        exception.Throw();
        return default!;
    }

    /// <summary>
    /// Disposes the lazy container and releases its currently created value, if any.
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
            _generation++;
            valueToRelease = _value;
            hasValueToRelease = _hasValue;
            _value = default;
            _hasValue = false;
            _factoryException = null;
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

    private enum PublishResult
    {
        Published,
        Obsolete,
        Disposed,
    }

    private enum FailureResult
    {
        Current,
        Obsolete,
        Disposed,
    }

    private readonly record struct ResetResult(bool WasReset, bool HasValue, T? Value);
}
