namespace FclEx.Utils;

public class AsyncTimerTests
{
    [Fact]
    public async Task Constructor_Does_Not_Start_Timer()
    {
        var callbackEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var timer = new AsyncTimer(
            _ =>
            {
                callbackEntered.TrySetResult(true);
                return Task.CompletedTask;
            },
            TimeSpan.Zero,
            TimeSpan.FromMinutes(1));

        await Task.Delay(50);
        Assert.False(callbackEntered.Task.IsCompleted);

        var runTask = timer.RunAsync();
        await CompletesWithin(callbackEntered.Task);
        await timer.StopAsync();
        await runTask;
    }

    [Fact]
    public async Task Callbacks_Do_Not_Overlap()
    {
        var firstCallbackEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFirstCallback = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondCallbackEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var invocationCount = 0;
        await using var timer = new AsyncTimer(
            async _ =>
            {
                var invocation = Interlocked.Increment(ref invocationCount);
                if (invocation == 1)
                {
                    firstCallbackEntered.TrySetResult(true);
                    await releaseFirstCallback.Task;
                }
                else
                {
                    secondCallbackEntered.TrySetResult(true);
                }
            },
            TimeSpan.Zero,
            TimeSpan.FromMilliseconds(1));

        var runTask = timer.RunAsync();
        await CompletesWithin(firstCallbackEntered.Task);
        await Task.Delay(50);
        Assert.Equal(1, Volatile.Read(ref invocationCount));

        releaseFirstCallback.SetResult(true);
        await CompletesWithin(secondCallbackEntered.Task);
        await timer.StopAsync();
        await runTask;
    }

    [Fact]
    public async Task Unhandled_Callback_Exception_Faults_Completion()
    {
        var expected = new InvalidOperationException("failed");
        var timer = new AsyncTimer(
            _ => Task.FromException(expected),
            TimeSpan.Zero,
            TimeSpan.FromMinutes(1));

        var runTask = timer.RunAsync();
        var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => runTask);
        var disposalFailure = await Assert.ThrowsAsync<InvalidOperationException>(() => timer.DisposeAsync().AsTask());

        Assert.Same(expected, actual);
        Assert.Same(expected, disposalFailure);
        Assert.Same(runTask, timer.Completion);
        Assert.False(timer.IsRunning);
    }

    [Fact]
    public async Task Exception_Handler_Allows_Timer_To_Continue()
    {
        var expected = new InvalidOperationException("failed");
        var secondCallbackEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        Exception? handledException = null;
        var invocationCount = 0;
        await using var timer = new AsyncTimer(
            async cancellationToken =>
            {
                if (Interlocked.Increment(ref invocationCount) == 1)
                    throw expected;

                secondCallbackEntered.TrySetResult(true);
                await Task.Delay(Timeout.Infinite, cancellationToken);
            },
            TimeSpan.Zero,
            TimeSpan.FromMilliseconds(1),
            (exception, _) =>
            {
                handledException = exception;
                return Task.CompletedTask;
            });

        var runTask = timer.RunAsync();
        await CompletesWithin(secondCallbackEntered.Task);
        await timer.StopAsync();
        await runTask;

        Assert.Same(expected, handledException);
        Assert.Equal(2, invocationCount);
    }

    [Fact]
    public async Task Caller_Cancellation_Cancels_Completion()
    {
        using var cancellation = new CancellationTokenSource();
        var timer = new AsyncTimer(
            _ => Task.CompletedTask,
            TimeSpan.FromMinutes(1),
            TimeSpan.FromMinutes(1));
        var runTask = timer.RunAsync(cancellation.Token);

        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => runTask);
        await timer.DisposeAsync();
    }

    [Fact]
    public async Task DisposeAsync_Cancels_And_Waits_For_Active_Callback()
    {
        var callbackEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var timer = new AsyncTimer(
            async cancellationToken =>
            {
                callbackEntered.TrySetResult(true);
                await Task.Delay(Timeout.Infinite, cancellationToken);
            },
            TimeSpan.Zero,
            TimeSpan.FromMinutes(1));
        var runTask = timer.RunAsync();
        await CompletesWithin(callbackEntered.Task);

        await CompletesWithin(timer.DisposeAsync().AsTask());
        await runTask;

        Assert.False(timer.IsRunning);
        Assert.Throws<ObjectDisposedException>(() =>
        {
            _ = timer.RunAsync();
        });
    }

    [Theory]
    [InlineData(-1, 1)]
    [InlineData(0, 0)]
    public void Constructor_Rejects_Invalid_Intervals(int dueTimeMilliseconds, int periodMilliseconds)
    {
        Assert.ThrowsAny<ArgumentException>(() => new AsyncTimer(
            _ => Task.CompletedTask,
            TimeSpan.FromMilliseconds(dueTimeMilliseconds),
            TimeSpan.FromMilliseconds(periodMilliseconds)));
    }

    private static async Task CompletesWithin(Task task)
    {
        var completedTask = await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(10)));
        Assert.Same(task, completedTask);
        await task;
    }
}
