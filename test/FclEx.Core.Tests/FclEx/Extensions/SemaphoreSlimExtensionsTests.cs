namespace FclEx.Extensions;

public class SemaphoreSlimExtensionsTests
{
    [Fact]
    public async Task WaitAsync_AcquiresRequestedPermits()
    {
        using var semaphore = new SemaphoreSlim(3, 3);

        var acquired = await semaphore.WaitAsync(2, TimeSpan.Zero);

        Assert.True(acquired);
        Assert.Equal(1, semaphore.CurrentCount);
    }

    [Fact]
    public async Task WaitAsync_TimeoutReleasesPermitsAcquiredByCall()
    {
        using var semaphore = new SemaphoreSlim(2, 3);

        var acquired = await semaphore.WaitAsync(3, TimeSpan.FromMilliseconds(10));

        Assert.False(acquired);
        Assert.Equal(2, semaphore.CurrentCount);
    }

    [Fact]
    public async Task WaitAsync_CancellationReleasesPermitsAcquiredByCall()
    {
        using var semaphore = new SemaphoreSlim(1, 2);
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(20));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => semaphore.WaitAsync(2, TimeSpan.FromSeconds(10), cancellation.Token));

        Assert.Equal(1, semaphore.CurrentCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task WaitAsync_NonPositiveCountThrows(int count)
    {
        using var semaphore = new SemaphoreSlim(1, 1);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => semaphore.WaitAsync(count, TimeSpan.Zero));
    }
}
