namespace FclEx.Utils.Disposables;

public class AsyncDisposableTests
{
    [Fact]
    public async Task Concurrent_DisposeAsync_Calls_Share_The_Same_Disposal()
    {
        var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var invocationCount = 0;
        var disposable = new AsyncDisposable(async () =>
        {
            Interlocked.Increment(ref invocationCount);
            entered.TrySetResult(true);
            await release.Task;
        });

        var firstDisposal = disposable.DisposeAsync().AsTask();
        await entered.Task;
        var secondDisposal = disposable.DisposeAsync().AsTask();

        Assert.False(secondDisposal.IsCompleted);
        release.SetResult(true);
        await Task.WhenAll(firstDisposal, secondDisposal);
        Assert.Equal(1, invocationCount);
    }

    [Fact]
    public async Task Failed_Disposal_Is_Shared_And_Not_Retried()
    {
        var expected = new InvalidOperationException("failed");
        var invocationCount = 0;
        var disposable = new AsyncDisposable(() =>
        {
            invocationCount++;
            return Task.FromException(expected);
        });

        var first = await Assert.ThrowsAsync<InvalidOperationException>(() => disposable.DisposeAsync().AsTask());
        var second = await Assert.ThrowsAsync<InvalidOperationException>(() => disposable.DisposeAsync().AsTask());

        Assert.Same(expected, first);
        Assert.Same(expected, second);
        Assert.Equal(1, invocationCount);
    }
}
