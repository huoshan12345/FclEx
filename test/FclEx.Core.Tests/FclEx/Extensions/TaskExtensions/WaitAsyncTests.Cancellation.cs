namespace FclEx.Extensions.TaskExtensions;

public partial class WaitAsyncTests
{
    [Theory]
    [MemberData(nameof(CancellationOverloads))]
    public async Task WaitAsync_PreCanceledToken_CancelsWaitWithCallerToken(WaitOverload overload)
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        var source = CreateSource();

        var wait = Wait(source.Task, overload, cancellationToken: cancellation.Token);
        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => AwaitWithWatchdog(wait));

        Assert.True(wait.IsCanceled);
        Assert.Equal(cancellation.Token, exception.CancellationToken);
        Assert.False(source.Task.IsCompleted);
        source.SetResult(42);
        Assert.Equal(42, await source.Task);
    }

    [Theory]
    [MemberData(nameof(CancellationOverloads))]
    public async Task WaitAsync_CallerCancelsPendingWait_DoesNotCancelSource(WaitOverload overload)
    {
        using var cancellation = new CancellationTokenSource();
        var source = CreateSource();
        var wait = Wait(source.Task, overload, TimeSpan.FromSeconds(30), cancellation.Token);
        Assert.False(wait.IsCompleted);

        await cancellation.CancelAsync();
        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => AwaitWithWatchdog(wait));

        Assert.True(wait.IsCanceled);
        Assert.Equal(cancellation.Token, exception.CancellationToken);
        Assert.False(source.Task.IsCompleted);
        source.SetResult(42);
        Assert.Equal(42, await source.Task);
        Assert.True(wait.IsCanceled);
    }

    [Theory]
    [InlineData(WaitOverload.TaskTimeoutAndCancellation)]
    [InlineData(WaitOverload.GenericTimeoutAndCancellation)]
    public async Task WaitAsync_PreCanceledToken_TakesPrecedenceOverZeroTimeout(WaitOverload overload)
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        var source = CreateSource();

        var wait = Wait(source.Task, overload, TimeSpan.Zero, cancellation.Token);
        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => AwaitWithWatchdog(wait));

        Assert.True(wait.IsCanceled);
        Assert.Equal(cancellation.Token, exception.CancellationToken);
        Assert.False(source.Task.IsCompleted);
        source.SetResult(42);
    }

    [Theory]
    [MemberData(nameof(CancellationOverloads))]
    public async Task WaitAsync_CancelingOneWait_DoesNotCancelAnotherWait(WaitOverload overload)
    {
        using var firstCancellation = new CancellationTokenSource();
        using var secondCancellation = new CancellationTokenSource();
        var source = CreateSource();
        var firstWait = Wait(source.Task, overload, cancellationToken: firstCancellation.Token);
        var secondWait = Wait(source.Task, overload, cancellationToken: secondCancellation.Token);

        await firstCancellation.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => AwaitWithWatchdog(firstWait));
        Assert.False(source.Task.IsCompleted);
        Assert.False(secondWait.IsCompleted);

        source.SetResult(42);
        await AssertSuccessfulWait(secondWait, overload);
        Assert.True(firstWait.IsCanceled);
        Assert.False(secondCancellation.IsCancellationRequested);
    }
}
