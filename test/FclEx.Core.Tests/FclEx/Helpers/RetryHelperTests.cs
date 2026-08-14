namespace FclEx.Helpers;

public class RetryHelperTests
{
    [Fact]
    public void Execute_Retries_Until_The_Operation_Succeeds()
    {
        var attemptCount = 0;

        var result = RetryHelper.Execute(
            _ => ++attemptCount < 3
                ? throw new InvalidOperationException("failed")
                : 42,
            maxRetryCount: 2);

        Assert.Equal(42, result);
        Assert.Equal(3, attemptCount);
    }

    [Fact]
    public void Execute_Does_Not_Delay_After_The_Final_Failure()
    {
        var attemptCount = 0;

        Assert.Throws<InvalidOperationException>(() => RetryHelper.Execute(
            _ =>
            {
                attemptCount++;
                throw new InvalidOperationException("failed");
            },
            maxRetryCount: 0,
            retryDelay: TimeSpan.FromSeconds(30)));

        Assert.Equal(1, attemptCount);
    }

    [Fact]
    public void Execute_Stops_When_The_Failure_Is_Not_Retryable()
    {
        var attemptCount = 0;

        Assert.Throws<InvalidOperationException>(() => RetryHelper.Execute(
            _ =>
            {
                attemptCount++;
                throw new InvalidOperationException("failed");
            },
            shouldRetry: _ => false));

        Assert.Equal(1, attemptCount);
    }

    [Fact]
    public async Task ExecuteAsync_Retries_Until_The_Operation_Succeeds()
    {
        var attemptCount = 0;

        var result = await RetryHelper.ExecuteAsync(
            _ => ++attemptCount < 3
                ? Task.FromException<int>(new InvalidOperationException("failed"))
                : Task.FromResult(42),
            maxRetryCount: 2);

        Assert.Equal(42, result);
        Assert.Equal(3, attemptCount);
    }

    [Fact]
    public async Task ExecuteAsync_Does_Not_Retry_Cancellation_From_The_Operation()
    {
        using var cancellation = new CancellationTokenSource();
        var attemptCount = 0;

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => RetryHelper.ExecuteAsync(
            async token =>
            {
                attemptCount++;
                cancellation.Cancel();
                await Task.Delay(TimeSpan.FromSeconds(30), token);
            },
            maxRetryCount: 3,
            cancellationToken: cancellation.Token));

        Assert.Equal(1, attemptCount);
    }

    [Fact]
    public async Task ExecuteAsync_Cancellation_Interrupts_Retry_Delay()
    {
        using var cancellation = new CancellationTokenSource();
        var attempted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var attemptCount = 0;
        var execution = RetryHelper.ExecuteAsync(
            _ =>
            {
                attemptCount++;
                attempted.TrySetResult(true);
                return Task.FromException(new InvalidOperationException("failed"));
            },
            maxRetryCount: 3,
            retryDelay: TimeSpan.FromSeconds(30),
            cancellationToken: cancellation.Token);

        await attempted.Task;
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => execution);
        Assert.Equal(1, attemptCount);
    }

    [Fact]
    public async Task ExecuteAsync_Rethrows_The_Final_Failure()
    {
        var expected = new InvalidOperationException("failed");
        var attemptCount = 0;

        var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => RetryHelper.ExecuteAsync(
            _ =>
            {
                attemptCount++;
                return Task.FromException(expected);
            },
            maxRetryCount: 1));

        Assert.Same(expected, actual);
        Assert.Equal(2, attemptCount);
    }

    [Fact]
    public void Execute_Rejects_A_Null_Operation()
    {
        Assert.Throws<ArgumentNullException>(() => RetryHelper.Execute(null!));
    }

    [Fact]
    public async Task ExecuteAsync_Rejects_A_Null_Operation()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => RetryHelper.ExecuteAsync(null!));
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    public async Task ExecuteAsync_Rejects_Negative_Policy_Values(int maxRetryCount, int retryDelayMilliseconds)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => RetryHelper.ExecuteAsync(
            _ => Task.CompletedTask,
            maxRetryCount,
            TimeSpan.FromMilliseconds(retryDelayMilliseconds)));
    }
}
