namespace FclEx.Extensions.TaskExtensions;

public partial class WaitAsyncTests
{
    public static IEnumerable<object[]> InvalidTimeoutCases()
    {
        long[] invalidTicks =
        [
            TimeSpan.MinValue.Ticks,
            -2 * TimeSpan.TicksPerMillisecond,
            (long)uint.MaxValue * TimeSpan.TicksPerMillisecond,
            TimeSpan.MaxValue.Ticks,
        ];

        foreach (var row in TimeoutOverloads)
        foreach (var ticks in invalidTicks)
        {
            yield return [row.Data, ticks, false];
            yield return [row.Data, ticks, true];
        }
    }

    public static IEnumerable<object[]> ValidTimeoutCases()
    {
        // The BCL truncates to whole milliseconds and permits up to uint.MaxValue - 1 ms.
        long[] validTicks =
        [
            Timeout.InfiniteTimeSpan.Ticks,
            -2 * TimeSpan.TicksPerMillisecond + 1,
            ((long)int.MaxValue + 1) * TimeSpan.TicksPerMillisecond,
            (uint.MaxValue - 1L) * TimeSpan.TicksPerMillisecond,
            (uint.MaxValue - 1L) * TimeSpan.TicksPerMillisecond + TimeSpan.TicksPerMillisecond - 1,
        ];

        foreach (var row in TimeoutOverloads)
        foreach (var ticks in validTicks)
            yield return [row.Data, ticks];
    }

    public static IEnumerable<object[]> ZeroTimeoutCases()
    {
        foreach (var row in TimeoutOverloads)
        foreach (var ticks in new long[] { -1, 0, TimeSpan.TicksPerMillisecond - 1 })
            yield return [row.Data, ticks];
    }

    [Theory]
    [MemberData(nameof(InvalidTimeoutCases))]
    public void WaitAsync_InvalidTimeout_ThrowsSynchronouslyBeforeCompletionOrCancellation(WaitOverload overload, long timeoutTicks, bool completedBeforeWait)
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var source = CreateSource();
        if (completedBeforeWait)
            source.SetResult(42);

        Assert.Throws<ArgumentOutOfRangeException>("timeout", () =>
        {
            _ = Wait(source.Task, overload, TimeSpan.FromTicks(timeoutTicks), cancellation.Token);
        });
    }

    [Theory]
    [MemberData(nameof(ValidTimeoutCases))]
    public async Task WaitAsync_ValidTimeout_AllowsSourceToComplete(WaitOverload overload, long timeoutTicks)
    {
        using var cancellation = new CancellationTokenSource();
        var source = CreateSource();

        var wait = Wait(source.Task, overload, TimeSpan.FromTicks(timeoutTicks), cancellation.Token);
        source.SetResult(42);

        await AssertSuccessfulWait(wait, overload);
    }

    [Theory]
    [MemberData(nameof(ZeroTimeoutCases))]
    public async Task WaitAsync_ZeroOrSubMillisecondTimeout_FaultsImmediately(WaitOverload overload, long timeoutTicks)
    {
        var source = CreateSource();
        var wait = Wait(source.Task, overload, TimeSpan.FromTicks(timeoutTicks));
        try
        {
            Assert.True(wait.IsCompleted);
            await Assert.ThrowsAsync<TimeoutException>(() => AwaitWithWatchdog(wait));
            Assert.True(wait.IsFaulted);
            Assert.False(source.Task.IsCompleted);
        }
        finally
        {
            source.TrySetResult(42);
        }
    }

    [Theory]
    [MemberData(nameof(TimeoutOverloads))]
    public async Task WaitAsync_TimeoutExpires_DoesNotCompleteSourceOrCancelCallerToken(WaitOverload overload)
    {
        using var cancellation = new CancellationTokenSource();
        var source = CreateSource();
        var wait = Wait(source.Task, overload, TimeSpan.FromMilliseconds(100), cancellation.Token);

        await Assert.ThrowsAsync<TimeoutException>(() => AwaitWithWatchdog(wait));

        Assert.True(wait.IsFaulted);
        Assert.False(source.Task.IsCompleted);
        Assert.False(cancellation.IsCancellationRequested);
        source.SetResult(42);
        Assert.Equal(42, await source.Task);
        Assert.True(wait.IsFaulted);
    }

    [Theory]
    [MemberData(nameof(TimeoutOverloads))]
    public async Task WaitAsync_TimingOutOneWait_DoesNotCompleteAnotherWait(WaitOverload overload)
    {
        var source = CreateSource();
        var firstWait = Wait(source.Task, overload, TimeSpan.FromMilliseconds(100));
        var secondWait = Wait(source.Task, overload, Timeout.InfiniteTimeSpan);

        await Assert.ThrowsAsync<TimeoutException>(() => AwaitWithWatchdog(firstWait));
        Assert.False(secondWait.IsCompleted);

        source.SetResult(42);
        await AssertSuccessfulWait(secondWait, overload);
        Assert.True(firstWait.IsFaulted);
    }
}
