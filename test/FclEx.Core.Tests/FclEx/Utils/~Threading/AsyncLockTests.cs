namespace FclEx.Utils;

public class AsyncLockTests
{
    [Fact]
    public void Acquire_DisposeLease_AllowsLockToBeAcquiredAgain()
    {
        var asyncLock = new AsyncLock();

        asyncLock.Acquire().Dispose();

        using var lease = asyncLock.Acquire();
    }

    [Fact]
    public async Task AcquireAsync_ReturnsNonDisposableAcquisitionAndDisposableLease()
    {
        var asyncLock = new AsyncLock();

        var acquisition = asyncLock.AcquireAsync();

        Assert.False(typeof(IDisposable).IsAssignableFrom(typeof(AsyncLock.Acquisition)));
        using var lease = await acquisition;
        Assert.IsAssignableFrom<IDisposable>(lease);
    }

    [Fact]
    public async Task AcquireAsync_ConfigureAwait_ReturnsLease()
    {
        var asyncLock = new AsyncLock();

        using var lease = await asyncLock.AcquireAsync().ConfigureAwait(false);

        Assert.NotNull(lease);
    }

    [Fact]
    public async Task AcquireAsync_WhenLockIsHeld_WaitsUntilLeaseIsDisposed()
    {
        var asyncLock = new AsyncLock();
        var firstLease = asyncLock.Acquire();
        var waitStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var acquired = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var waitingTask = Task.Run(async () =>
        {
            waitStarted.SetResult(true);
            using (await asyncLock.AcquireAsync())
                acquired.SetResult(true);
        });

        await waitStarted.Task;
        Assert.False(acquired.Task.IsCompleted);

        firstLease.Dispose();

        await acquired.Task;
        await waitingTask;
    }

    [Fact]
    public async Task AcquireAsync_ConcurrentCallers_AllowOnlyOneCallerAtATime()
    {
        var asyncLock = new AsyncLock();
        var callersInsideLock = 0;

        var tasks = Enumerable.Range(0, 100)
            .Select(async _ =>
            {
                using (await asyncLock.AcquireAsync())
                {
                    Assert.Equal(1, Interlocked.Increment(ref callersInsideLock));
                    await Task.Yield();
                    Interlocked.Decrement(ref callersInsideLock);
                }
            });

        await Task.WhenAll(tasks);

        Assert.Equal(0, callersInsideLock);
    }

    [Fact]
    public void Acquire_WithCanceledToken_DoesNotAcquireLock()
    {
        var asyncLock = new AsyncLock();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Assert.ThrowsAny<OperationCanceledException>(() => asyncLock.Acquire(cancellation.Token));

        using var lease = asyncLock.Acquire();
    }

    [Fact]
    public async Task AcquireAsync_WithCanceledToken_DoesNotAcquireLock()
    {
        var asyncLock = new AsyncLock();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            using (await asyncLock.AcquireAsync(cancellation.Token)) { }
        });

        Assert.Equal(cancellation.Token, exception.CancellationToken);
        using var lease = asyncLock.Acquire();
    }

    [Fact]
    public async Task AcquireAsync_WhenCanceledWhileWaiting_DoesNotAcquireLock()
    {
        var asyncLock = new AsyncLock();
        using var firstLease = asyncLock.Acquire();
        using var cancellation = new CancellationTokenSource();

        var waitingTask = AcquireAndReleaseAsync(asyncLock, cancellation.Token);
        cancellation.Cancel();

        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => waitingTask);
        Assert.Equal(cancellation.Token, exception.CancellationToken);

        firstLease.Dispose();
        using var nextLease = asyncLock.Acquire();
    }

    [Fact]
    public async Task Lease_DisposeMoreThanOnce_ReleasesLockOnlyOnce()
    {
        var asyncLock = new AsyncLock();
        var firstLease = asyncLock.Acquire();

        firstLease.Dispose();
        firstLease.Dispose();

        using var secondLease = asyncLock.Acquire();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            using (await asyncLock.AcquireAsync(cancellation.Token)) { }
        });
    }

    [Fact]
    public async Task Lease_WhenProtectedOperationThrows_ReleasesLock()
    {
        var asyncLock = new AsyncLock();

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            using (await asyncLock.AcquireAsync())
                throw new InvalidOperationException();
        });

        using var lease = await asyncLock.AcquireAsync();
    }

    [Fact]
    public async Task DefaultAcquisition_CannotBeAwaited()
    {
        var acquisition = default(AsyncLock.Acquisition);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            using (await acquisition) { }
        });

        Assert.Contains("uninitialized", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task AcquireAndReleaseAsync(AsyncLock asyncLock, CancellationToken cancellationToken)
    {
        using (await asyncLock.AcquireAsync(cancellationToken)) { }
    }
}
