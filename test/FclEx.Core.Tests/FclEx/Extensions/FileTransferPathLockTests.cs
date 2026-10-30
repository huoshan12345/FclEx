namespace FclEx.Extensions;

public class FileTransferPathLockTests
{
    public static TheoryData<string, bool> PathCases => new()
    {
        { "report.txt", true },
        { Path.Combine("sub", "..", "report.txt"), true },
        { "REPORT.TXT", RuntimeInformation.IsOSPlatform(OSPlatform.Windows) },
        { "another.txt", false },
    };

    [Theory]
    [MemberData(nameof(PathCases))]
    public async Task Acquire_CoordinatesOnlyMatchingPaths(string secondName, bool shouldBlock)
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var firstPath = Path.Combine(directory, "report.txt");
        var secondPath = Path.Combine(directory, secondName);
        using var started = new ManualResetEventSlim();
        using var acquired = new ManualResetEventSlim();
        Task worker;
        bool startedWhileHeld;
        bool acquiredWhileHeld;

        using (FileTransferPathLock.Acquire(firstPath))
        {
            worker = Task.Factory.StartNew(() =>
            {
                started.Set();
                using var lease = FileTransferPathLock.Acquire(secondPath);
                acquired.Set();
            }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            startedWhileHeld = started.Wait(TimeSpan.FromSeconds(10));
            acquiredWhileHeld = acquired.Wait(shouldBlock
                ? TimeSpan.FromMilliseconds(100)
                : TimeSpan.FromSeconds(10));
        }

        await worker.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.True(startedWhileHeld);
        Assert.Equal(!shouldBlock, acquiredWhileHeld);
        Assert.True(acquired.IsSet);
    }

    [Theory]
    [MemberData(nameof(PathCases))]
    public async Task AcquireAsync_CoordinatesWithSynchronousAcquisition(string secondName, bool shouldBlock)
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var firstPath = Path.Combine(directory, "report.txt");
        var secondPath = Path.Combine(directory, secondName);
        Task<IDisposable> waiting;
        bool completedWhileHeld;

        using (FileTransferPathLock.Acquire(firstPath))
        {
            // Calling this on the holder's thread must return a pending task, not block or reenter.
            waiting = FileTransferPathLock.AcquireAsync(secondPath);
            completedWhileHeld = waiting.IsCompleted;
        }

        using var lease = await waiting.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Equal(!shouldBlock, completedWhileHeld);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task AcquireAsync_CanceledWait_DoesNotReplaceHeldPathLock(bool cancelBeforeWaiting)
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        using var cancellation = new CancellationTokenSource();
        Task<IDisposable> next;
        bool nextWaited;

        using (FileTransferPathLock.Acquire(path))
        {
            if (cancelBeforeWaiting)
                await cancellation.CancelAsync();

            var waiting = FileTransferPathLock.AcquireAsync(path, cancellation.Token);
            if (!cancelBeforeWaiting)
            {
                Assert.False(waiting.IsCompleted);
                await cancellation.CancelAsync();
            }

            var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => waiting.WaitAsync(TimeSpan.FromSeconds(10)));
            Assert.Equal(cancellation.Token, exception.CancellationToken);

            // Canceling a waiter must leave the holder's entry in the registry for later callers.
            next = FileTransferPathLock.AcquireAsync(path);
            nextWaited = !next.IsCompleted;
        }

        using var lease = await next.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.True(nextWaited);
    }

    [Fact]
    public async Task Lease_DisposeFromOtherThreads_ReleasesOnlyOnce()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        using var first = FileTransferPathLock.Acquire(path);

        await Task.WhenAll(
            Task.Factory.StartNew(first.Dispose, CancellationToken.None,
                TaskCreationOptions.LongRunning, TaskScheduler.Default),
            Task.Factory.StartNew(first.Dispose, CancellationToken.None,
                TaskCreationOptions.LongRunning, TaskScheduler.Default));

        Task<IDisposable> waiting;
        bool waited;
        using (await FileTransferPathLock.AcquireAsync(path))
        {
            first.Dispose();
            waiting = FileTransferPathLock.AcquireAsync(path);
            waited = !waiting.IsCompleted;
        }

        using var lease = await waiting.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.True(waited);
    }

    [Fact]
    public async Task Acquire_RepeatedContention_RemainsExclusive()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var active = 0;
        var violations = 0;
        using var start = new Barrier(4);
        var workers = Enumerable.Range(0, 4).Select(_ => Task.Factory.StartNew(() =>
        {
            if (!start.SignalAndWait(TimeSpan.FromSeconds(30)))
                throw new TimeoutException("The lock workers did not reach the start barrier.");

            for (var iteration = 0; iteration < 500; iteration++)
            {
                using var lease = FileTransferPathLock.Acquire(path);
                if (Interlocked.Increment(ref active) != 1)
                    Interlocked.Increment(ref violations);
                Thread.Yield();
                Interlocked.Decrement(ref active);
            }
        }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default)).ToArray();

        await Task.WhenAll(workers).WaitAsync(TimeSpan.FromSeconds(30));
        Assert.Equal(0, violations);
        Assert.Equal(0, active);
    }

    [Fact]
    public async Task Acquire_ExceptionInProtectedOperation_ReleasesPath()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Assert.Throws<InvalidOperationException>((Action)(() =>
        {
            using var lease = FileTransferPathLock.Acquire(path);
            throw new InvalidOperationException("Simulated transfer failure.");
        }));

        await Task.Factory.StartNew(() =>
        {
            using var lease = FileTransferPathLock.Acquire(path);
        }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default)
            .WaitAsync(TimeSpan.FromSeconds(10));
    }
}
