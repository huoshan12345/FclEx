namespace FclEx.Utils;

partial class OperationTests
{
    [RetryFact(3, 100)]
    public async Task ExecuteAsync_Timeout_Test()
    {
        var (success, exception, elapsed) = await Operation.ExecuteAsync(()
            => Task.Delay(TimeSpan.FromSeconds(5)), TimeSpan.FromSeconds(0.1));

        Assert.False(success);
        Assert.True(elapsed < TimeSpan.FromSeconds(1.5), () => $"Expected {nameof(elapsed)} < {TimeSpan.FromSeconds(1.5)}, but was {elapsed}");
#if NET5_0_OR_GREATER
        Assert.IsType<TimeoutException>(exception);
#else
        Assert.IsType<OperationCanceledException>(exception);
#endif
    }

    [RetryFact(3, 100)]
    public async Task ExecuteAsync_Timeout_Success_Test()
    {
        var (success, result, _, elapsed) = await Operation.ExecuteAsync(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(0.1));
            return 1;
        }, TimeSpan.FromSeconds(10));

        Assert.True(success);
        Assert.Equal(1, result);
        Assert.True(elapsed < TimeSpan.FromSeconds(1.5), () => $"Expected {nameof(elapsed)} < {TimeSpan.FromSeconds(1.5)}, but was {elapsed}");
    }

    [RetryFact(3, 100)]
    public async Task ExecuteAsync_Timeout_SyncBody_Test()
    {
        var (success, exception, elapsed) = await Operation.ExecuteAsync(() =>
        {
            ThreadHelper.Sleep(10);
            return Task.CompletedTask;
        }, TimeSpan.FromSeconds(0.1));

        Assert.False(success);
        Assert.True(elapsed < TimeSpan.FromSeconds(1.5), () => $"Expected {nameof(elapsed)} < {TimeSpan.FromSeconds(1.5)}, but was {elapsed}");
#if NET5_0_OR_GREATER
        Assert.IsType<TimeoutException>(exception);
#else
        Assert.IsType<OperationCanceledException>(exception);
#endif
    }

    [RetryFact(3, 100)]
    public async Task ExecuteAsync_Timeout_SyncBody_Success_Test()
    {
        var (success, result, _, elapsed) = await Operation.ExecuteAsync(() =>
        {
            ThreadHelper.Sleep(0.1);
            return Task.FromResult(1);
        }, TimeSpan.FromSeconds(1));

        Assert.True(success);
        Assert.Equal(1, result);
        Assert.True(elapsed < TimeSpan.FromSeconds(1.5), () => $"Expected {nameof(elapsed)} < {TimeSpan.FromSeconds(1.5)}, but was {elapsed}");
    }

    [RetryFact(3, 100)]
    public async Task ExecuteValueAsync_OperationResult_Timeout_Test()
    {
        var (success, _, exception, elapsed) = await Operation.ExecuteValueAsync(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            return Operation.Success(1);
        }, TimeSpan.FromSeconds(0.1));

        Assert.False(success);
        Assert.True(elapsed < TimeSpan.FromSeconds(1.5), () => $"Expected {nameof(elapsed)} < {TimeSpan.FromSeconds(1.5)}, but was {elapsed}");
#if NET5_0_OR_GREATER
        Assert.IsType<TimeoutException>(exception);
#else
        Assert.IsType<OperationCanceledException>(exception);
#endif
    }

    [Fact]
    public async Task ExecuteAsync_OperationResult_UsesOuterElapsed()
    {
        var r = await Operation.ExecuteAsync(() => Task.FromResult(Operation.Success(TimeSpan.FromHours(1))));

        Assert.True(r.IsSuccess);
        Assert.NotEqual(TimeSpan.FromHours(1), r.Elapsed);
        Assert.True(r.Elapsed < TimeSpan.FromMinutes(1), () => $"Expected {nameof(r.Elapsed)} < {TimeSpan.FromMinutes(1)}, but was {r.Elapsed}");
    }
}
