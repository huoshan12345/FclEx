namespace FclEx.Utils.OperationResult;

partial class OperationResultTests
{
    [RetryFact]
    public async Task ExecuteAsync_Timeout_Test()
    {
        var (success, exception, elapsed) = await Operation.ExecuteAsync(() => Task.Delay(TimeSpan.FromSeconds(5)), TimeSpan.FromSeconds(1));
        Assert.False(success);
        Assert.True(elapsed < TimeSpan.FromSeconds(1.5), elapsed.ToString());
        Assert.IsType<TimeoutException>(exception);
    }

    [RetryFact]
    public async Task ExecuteAsync_Timeout_Success_Test()
    {
        var (success, result, _, elapsed) = await Operation.ExecuteAsync(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            return 1;
        }, TimeSpan.FromSeconds(10));
        Assert.True(success);
        Assert.Equal(1, result);
        Assert.True(elapsed < TimeSpan.FromSeconds(1.5), elapsed.ToString());
    }

    [RetryFact]
    public async Task ExecuteAsync_Timeout_SyncBody_Test()
    {
        var (success, exception, elapsed) = await Operation.ExecuteAsync(() =>
        {
            ThreadHelper.Sleep(10);
            return Task.CompletedTask;
        }, TimeSpan.FromSeconds(1));
        Assert.False(success);
        Assert.True(elapsed < TimeSpan.FromSeconds(1.5), elapsed.ToString());
        Assert.IsType<TimeoutException>(exception);
    }

    [RetryFact]
    public async Task ExecuteAsync_Timeout_SyncBody_Success_Test()
    {
        var (success, result, _, elapsed) = await Operation.ExecuteAsync(() =>
        {
            ThreadHelper.Sleep(1);
            return Task.FromResult(1);
        }, TimeSpan.FromSeconds(10));

        Assert.True(success);
        Assert.Equal(1, result);
        Assert.True(elapsed < TimeSpan.FromSeconds(1.5), elapsed.ToString());
    }
}