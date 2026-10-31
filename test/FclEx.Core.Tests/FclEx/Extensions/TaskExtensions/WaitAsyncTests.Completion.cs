namespace FclEx.Extensions.TaskExtensions;

public partial class WaitAsyncTests
{
    [Theory]
    [MemberData(nameof(AllOverloads))]
    public async Task WaitAsync_InfiniteTimeoutWithoutCancellation_WaitsForSource(WaitOverload overload)
    {
        var source = CreateSource();
        var wait = Wait(source.Task, overload);
        Assert.False(wait.IsCompleted);

        source.SetResult(42);

        await AssertSuccessfulWait(wait, overload);
    }

    [Theory]
    [MemberData(nameof(OverloadsAndCompletionStates))]
    public async Task WaitAsync_SourceSucceeds_PreservesResult(WaitOverload overload, bool completedBeforeWait)
    {
        using var cancellation = new CancellationTokenSource();
        var source = CreateSource();
        if (completedBeforeWait)
        {
            source.SetResult(42);
            cancellation.Cancel();
        }

        // Already-completed tasks take precedence over zero timeout and caller cancellation.
        var wait = Wait(source.Task, overload, completedBeforeWait ? TimeSpan.Zero : Timeout.InfiniteTimeSpan, cancellation.Token);
        if (!completedBeforeWait)
        {
            Assert.False(wait.IsCompleted);
            source.SetResult(42);
        }

        await AssertSuccessfulWait(wait, overload);
        cancellation.Cancel();
        Assert.Equal(TaskStatus.RanToCompletion, wait.Status);
    }

    [Theory]
    [MemberData(nameof(OverloadsAndCompletionStates))]
    public async Task WaitAsync_SourceFaults_PreservesAllExceptions(WaitOverload overload, bool completedBeforeWait)
    {
        using var cancellation = new CancellationTokenSource();
        var source = CreateSource();
        Exception[] errors = [new InvalidOperationException("first"), new ArgumentException("second")];
        if (completedBeforeWait)
        {
            source.SetException(errors);
            cancellation.Cancel();
        }

        var wait = Wait(source.Task, overload, completedBeforeWait ? TimeSpan.Zero : Timeout.InfiniteTimeSpan, cancellation.Token);
        if (!completedBeforeWait)
            source.SetException(errors);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => AwaitWithWatchdog(wait));

        Assert.Same(errors[0], exception);
        Assert.True(wait.IsFaulted);
        Assert.Equal(errors, wait.Exception!.InnerExceptions);
    }

    [Theory]
    [MemberData(nameof(OverloadsAndCompletionStates))]
    public async Task WaitAsync_SourceIsCanceled_PreservesSourceCancellationToken(WaitOverload overload, bool completedBeforeWait)
    {
        using var sourceCancellation = new CancellationTokenSource();
        using var waitCancellation = new CancellationTokenSource();
        sourceCancellation.Cancel();
        var source = CreateSource();
        if (completedBeforeWait)
        {
            Assert.True(source.TrySetCanceled(sourceCancellation.Token));
            waitCancellation.Cancel();
        }

        var wait = Wait(source.Task, overload, completedBeforeWait ? TimeSpan.Zero : Timeout.InfiniteTimeSpan, waitCancellation.Token);
        if (!completedBeforeWait)
            Assert.True(source.TrySetCanceled(sourceCancellation.Token));

        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => AwaitWithWatchdog(wait));

        Assert.True(wait.IsCanceled);
        Assert.Equal(sourceCancellation.Token, exception.CancellationToken);
    }

    [Theory]
    [MemberData(nameof(OverloadsAndCompletionStates))]
    public async Task WaitAsync_SourceFaultsWithOperationCanceledException_PreservesFaultedStatus(WaitOverload overload, bool completedBeforeWait)
    {
        using var cancellation = new CancellationTokenSource();
        var source = CreateSource();
        var error = new OperationCanceledException(cancellation.Token);
        if (completedBeforeWait)
            source.SetException(error);

        var wait = Wait(source.Task, overload, cancellationToken: cancellation.Token);
        if (!completedBeforeWait)
            source.SetException(error);

        var exception = await Assert.ThrowsAsync<OperationCanceledException>(() => AwaitWithWatchdog(wait));

        Assert.Same(error, exception);
        Assert.True(source.Task.IsFaulted);
        Assert.True(wait.IsFaulted);
        Assert.Same(error, Assert.Single(wait.Exception!.InnerExceptions));
    }

    [Fact]
    public async Task WaitAsync_GenericOverloads_PreserveNullResult()
    {
        using var cancellation = new CancellationTokenSource();
        var source = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
        Task<string?>[] waits =
        [
            source.Task.WaitAsync(cancellation.Token),
            source.Task.WaitAsync(Timeout.InfiniteTimeSpan),
            source.Task.WaitAsync(Timeout.InfiniteTimeSpan, cancellation.Token),
        ];
        source.SetResult(null);

        foreach (var wait in waits)
        {
            await AwaitWithWatchdog(wait);
            Assert.Null(await wait);
        }
    }
}
