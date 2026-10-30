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
