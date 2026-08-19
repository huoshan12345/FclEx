namespace FclEx.Extensions.TaskExtensions;

public class RunAsyncTests
{
    [RetryFact]
    public async Task RunAsync_ShouldCancelTheOperationAndThrowOnTimeout()
    {
        CancellationToken operationToken = default;

        await Assert.ThrowsAsync<TimeoutException>(() => Task.RunAsync(
            async token =>
            {
                operationToken = token;
                await Task.Delay(Timeout.InfiniteTimeSpan, token);
            },
            TimeSpan.FromMilliseconds(100), CancellationToken.None));

        Assert.True(operationToken.IsCancellationRequested);
    }

    [Fact]
    public async Task RunAsync_ShouldStopWaitingWhenTheOperationIgnoresTimeoutCancellation()
    {
        var operation = new TaskCompletionSource<object?>();

        await Assert.ThrowsAsync<TimeoutException>(() => Task.RunAsync(
            _ => operation.Task,
            TimeSpan.FromMilliseconds(100)));

        operation.TrySetResult(null);
    }

    [Fact]
    public async Task RunAsync_ShouldPropagateCallerCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        var invoked = false;

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Task.RunAsync(
            _ =>
            {
                invoked = true;
                return Task.CompletedTask;
            },
            cancellationToken: cancellation.Token));

        Assert.False(invoked);
    }

    [Fact]
    public async Task RunAsync_ValueTask_ShouldReturnResult()
    {
        var result = await Task.RunAsync(_ => ValueTask.FromResult(42).AsTask());

        Assert.Equal(42, result);
    }
}
