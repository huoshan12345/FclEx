namespace FclEx.Extensions.TaskExtensions;

public class WaitAsyncTests
{
    [Fact]
    public async Task WaitAsync_ShouldPreferAnAlreadyCompletedTaskOverCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Task.CompletedTask.WaitAsync(cancellation.Token);
    }

    [Fact]
    public async Task WaitAsync_ShouldThrowTimeoutExceptionWhenTheTimeoutExpires()
    {
        var task = new TaskCompletionSource<object?>().Task;

        await Assert.ThrowsAsync<TimeoutException>(() => task.WaitAsync(TimeSpan.FromMilliseconds(20)));
    }
}
