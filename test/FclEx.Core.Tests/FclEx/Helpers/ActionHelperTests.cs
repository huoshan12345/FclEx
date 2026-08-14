namespace FclEx.Helpers;

public class ActionHelperTests
{
    [Fact]
    public async Task TryAsync_Retries_Until_The_Operation_Succeeds()
    {
        var attemptCount = 0;

        var result = await ActionHelper.TryAsync(
            _ => ++attemptCount < 3
                ? Task.FromException<int>(new InvalidOperationException("failed"))
                : Task.FromResult(42),
            maxRetryCount: 2);

        Assert.Equal(42, result);
        Assert.Equal(3, attemptCount);
    }

    [Fact]
    public async Task TryAsync_Uses_Fallback_After_Retries_Are_Exhausted()
    {
        var expected = new InvalidOperationException("failed");

        var result = await ActionHelper.TryAsync<int>(
            _ => Task.FromException<int>(expected),
            maxRetryCount: 1,
            fallback: exception => exception == expected ? 42 : 0);

        Assert.Equal(42, result);
    }

    [Fact]
    public async Task TryAsync_Does_Not_Retry_Cancellation_From_The_Operation()
    {
        using var cancellation = new CancellationTokenSource();
        var attemptCount = 0;

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => ActionHelper.TryAsync(
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
    public async Task TryAsync_Cancellation_Interrupts_Retry_Delay()
    {
        using var cancellation = new CancellationTokenSource();
        var attempted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var attemptCount = 0;
        var execution = ActionHelper.TryAsync(
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
    public async Task TryAsync_Notifies_And_Rethrows_Final_Failure()
    {
        var expected = new InvalidOperationException("failed");
        Exception? observed = null;
        var attemptCount = 0;

        var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => ActionHelper.TryAsync(
            _ =>
            {
                attemptCount++;
                return Task.FromException(expected);
            },
            maxRetryCount: 1,
            onFailure: exception => observed = exception,
            throwOnFailure: true));

        Assert.Same(expected, actual);
        Assert.Same(expected, observed);
        Assert.Equal(2, attemptCount);
    }
}
