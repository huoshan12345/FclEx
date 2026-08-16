namespace FclEx.Extensions;

public class AsyncEnumerableExtensionsTests
{
    [Fact]
    public async Task ToListAsync_CanceledToken_ShouldCancelEnumeration()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => GetValues().ToListAsync(cancellationTokenSource.Token));
    }

    [Fact]
    public async Task ToArrayAsync_ShouldMaterializeValues()
    {
        var result = await GetValues().ToArrayAsync();

        Assert.Equal([1, 2], result);
    }

    private static async IAsyncEnumerable<int> GetValues([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        yield return 1;
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();
        yield return 2;
    }
}
