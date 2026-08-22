namespace FclEx.Utils;

/// <summary>
/// Provides mutually exclusive access that can be acquired synchronously or asynchronously.
/// </summary>
/// <remarks>
/// <para>
/// The lock is not reentrant. Attempting to acquire it again before disposing the current
/// <see cref="Lease"/> waits until that lease is disposed.
/// </para>
/// <para>
/// Every successful acquisition must be disposed. No ordering guarantee is made when multiple
/// callers are waiting to acquire the lock.
/// </para>
/// </remarks>
public sealed class AsyncLock
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    /// <summary>
    /// Waits synchronously until the lock can be acquired.
    /// </summary>
    /// <param name="cancellationToken">A token that can cancel the wait.</param>
    /// <returns>A lease that releases the lock when disposed.</returns>
    /// <exception cref="OperationCanceledException">
    /// <paramref name="cancellationToken"/> was canceled before the lock was acquired.
    /// </exception>
    public Lease Acquire(CancellationToken cancellationToken = default)
    {
        _semaphore.Wait(cancellationToken);
        return new Lease(_semaphore);
    }

    /// <summary>
    /// Returns an operation that asynchronously waits until the lock can be acquired.
    /// </summary>
    /// <param name="cancellationToken">A token that can cancel the wait.</param>
    /// <returns>
    /// An awaitable acquisition. Awaiting it produces a lease that releases the lock when disposed.
    /// The acquisition itself is intentionally not disposable, so it cannot be used in a
    /// <c>using</c> statement without first being awaited.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// Awaiting the acquisition was canceled before the lock was acquired.
    /// </exception>
    public Acquisition AcquireAsync(CancellationToken cancellationToken = default)
    {
        return new Acquisition(AcquireCoreAsync(cancellationToken));
    }

    private async Task<Lease> AcquireCoreAsync(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        return new Lease(_semaphore);
    }

    /// <summary>
    /// Represents an asynchronous attempt to acquire an <see cref="AsyncLock"/>.
    /// </summary>
    /// <remarks>
    /// This type is awaitable but not disposable. Await it to obtain the disposable
    /// <see cref="Lease"/> that owns the lock.
    /// </remarks>
    public readonly struct Acquisition
    {
        private readonly Task<Lease>? _task;

        internal Acquisition(Task<Lease> task)
        {
            _task = task;
        }

        /// <summary>
        /// Returns the awaiter for this acquisition.
        /// </summary>
        /// <returns>An awaiter that produces the acquired lease.</returns>
        public TaskAwaiter<Lease> GetAwaiter()
        {
            return GetTask().GetAwaiter();
        }

        /// <summary>
        /// Configures how the continuation is marshalled when awaiting this acquisition.
        /// </summary>
        /// <param name="continueOnCapturedContext">
        /// <see langword="true"/> to attempt to marshal the continuation back to the captured context;
        /// otherwise, <see langword="false"/>.
        /// </param>
        /// <returns>A configured awaitable for this acquisition.</returns>
        public ConfiguredTaskAwaitable<Lease> ConfigureAwait(bool continueOnCapturedContext)
        {
            return GetTask().ConfigureAwait(continueOnCapturedContext);
        }

        private Task<Lease> GetTask()
        {
            return _task ?? throw new InvalidOperationException("An uninitialized acquisition cannot be awaited.");
        }
    }

    /// <summary>
    /// Represents ownership of an acquired <see cref="AsyncLock"/>.
    /// </summary>
    /// <remarks>
    /// Disposing a lease releases the lock. Repeated disposal is safe and releases the lock only once.
    /// </remarks>
    public sealed class Lease : IDisposable
    {
        private SemaphoreSlim? _semaphore;

        internal Lease(SemaphoreSlim semaphore)
        {
            _semaphore = semaphore;
        }

        /// <summary>
        /// Releases the lock if this lease has not already been disposed.
        /// </summary>
        public void Dispose()
        {
            Interlocked.Exchange(ref _semaphore, null)?.Release();
        }
    }
}
