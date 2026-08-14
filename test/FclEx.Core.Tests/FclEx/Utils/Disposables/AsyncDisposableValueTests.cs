namespace FclEx.Utils.Disposables;

public class AsyncDisposableValueTests
{
    [Fact]
    public async Task Concurrent_DisposeAsync_Calls_Share_The_Same_Disposal()
    {
        var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var invocationCount = 0;
        var disposable = new AsyncDisposableValue<int>(42, async _ =>
        {
            Interlocked.Increment(ref invocationCount);
            entered.TrySetResult(true);
            await release.Task;
        });

        var firstDisposal = disposable.DisposeAsync().AsTask();
        await entered.Task;
        var secondDisposal = disposable.DisposeAsync().AsTask();

        Assert.Throws<ObjectDisposedException>(() => _ = disposable.Value);
        Assert.False(secondDisposal.IsCompleted);
        release.SetResult(true);
        await Task.WhenAll(firstDisposal, secondDisposal);
        Assert.Equal(1, invocationCount);
    }

    [Fact]
    public async Task Failed_Disposal_Is_Shared_And_Leaves_Value_Disposed()
    {
        var expected = new InvalidOperationException("failed");
        var invocationCount = 0;
        var disposable = new AsyncDisposableValue<int>(42, _ =>
        {
            invocationCount++;
            return new ValueTask(Task.FromException(expected));
        });

        var first = await Assert.ThrowsAsync<InvalidOperationException>(() => disposable.DisposeAsync().AsTask());
        var second = await Assert.ThrowsAsync<InvalidOperationException>(() => disposable.DisposeAsync().AsTask());

        Assert.Same(expected, first);
        Assert.Same(expected, second);
        Assert.Equal(1, invocationCount);
        Assert.Throws<ObjectDisposedException>(() => _ = disposable.Value);
    }
}
