namespace FclEx.Utils;

internal sealed class TrackedDisposable : IDisposable
{
    private int _disposeCount;

    public int DisposeCount => Volatile.Read(ref _disposeCount);

    public bool IsDisposed => DisposeCount != 0;

    public void Dispose() => Interlocked.Increment(ref _disposeCount);
}

internal static class LazyTestHelper
{
    public static async Task WaitUntilAsync(Func<bool> condition, TimeSpan timeout)
    {
        var stopwatch = Stopwatch.StartNew();
        while (condition() == false && stopwatch.Elapsed < timeout)
            await Task.Delay(10);

        Assert.True(condition(), $"The condition was not satisfied within {timeout}.");
    }
}
