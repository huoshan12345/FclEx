namespace FclEx.Utils;

public class TimerTests
{
    [Fact]
    public void NonCapturingTimer_Create_ReturnsGenericAndNonGenericTimer()
    {
        using var nonGenericTimer = NonCapturingTimer.Create(() => { }, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        using var genericTimer = NonCapturingTimer.Create<int>(_ => { }, 1, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

        Assert.IsType<Timer>(nonGenericTimer);
        Assert.IsType<Timer<int>>(genericTimer);
    }

    [Fact]
    public void Dispose_IsIdempotentAndUpdatesActiveState()
    {
        var statefulTimer = new Timer<int>(_ => { }, 1, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        var statelessTimer = new Timer(() => { }, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

        Assert.True(statefulTimer.IsActive);
        Assert.True(statelessTimer.IsActive);

        statefulTimer.Dispose();
        statelessTimer.Dispose();
        statefulTimer.Dispose();
        statelessTimer.Dispose();

        Assert.False(statefulTimer.IsActive);
        Assert.False(statelessTimer.IsActive);
    }

    [Fact]
    public async Task DisposeAsync_WaitsForAnActiveCallback()
    {
        var callbackStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseCallback = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var timer = new Timer(
            () =>
            {
                callbackStarted.TrySetResult();
                releaseCallback.Task.GetAwaiter().GetResult();
            },
            TimeSpan.Zero,
            Timeout.InfiniteTimeSpan);

        try
        {
            var started = await Task.WhenAny(callbackStarted.Task, Task.Delay(TimeSpan.FromSeconds(5)));
            Assert.Same(callbackStarted.Task, started);

            var disposal = timer.DisposeAsync().AsTask();
            Assert.False(disposal.IsCompleted);
            Assert.False(timer.IsActive);

            releaseCallback.TrySetResult();
            await disposal;
        }
        finally
        {
            releaseCallback.TrySetResult();
            await timer.DisposeAsync();
        }
    }

    [Fact]
    public async Task GenericDisposeAsync_WaitsForAnActiveCallback()
    {
        var callbackStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseCallback = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var timer = new Timer<int>(
            _ =>
            {
                callbackStarted.TrySetResult();
                releaseCallback.Task.GetAwaiter().GetResult();
            },
            42,
            TimeSpan.Zero,
            Timeout.InfiniteTimeSpan);

        try
        {
            var started = await Task.WhenAny(callbackStarted.Task, Task.Delay(TimeSpan.FromSeconds(5)));
            Assert.Same(callbackStarted.Task, started);

            var disposal = timer.DisposeAsync().AsTask();
            Assert.False(disposal.IsCompleted);

            releaseCallback.TrySetResult();
            await disposal;
        }
        finally
        {
            releaseCallback.TrySetResult();
            await timer.DisposeAsync();
        }
    }
}
