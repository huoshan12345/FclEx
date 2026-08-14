namespace FclEx.Extensions.EnumerableExtensions;

public class AsyncExecutionTests
{
    [Fact]
    public async Task ForEachSequentiallyAsync_Executes_In_Source_Order()
    {
        var actual = new List<int>();

        await Enumerable.Range(0, 5).ForEachSequentiallyAsync((item, _) =>
        {
            actual.Add(item);
            return default;
        });

        Assert.Equal(Enumerable.Range(0, 5), actual);
    }

    [Fact]
    public async Task SelectSequentiallyAsync_Returns_Results_In_Source_Order()
    {
        var results = await Enumerable.Range(0, 5)
            .SelectSequentiallyAsync((item, _) => new ValueTask<int>(item * 2));

        Assert.Equal([0, 2, 4, 6, 8], results);
    }

    [Fact]
    public async Task ForEachConcurrentlyAsync_Sustains_The_Maximum_Degree_Of_Parallelism()
    {
        var activeCount = 0;
        var maximumActiveCount = 0;
        var sync = new object();

        await Enumerable.Range(0, 20).ForEachConcurrentlyAsync(
            async (_, token) =>
            {
                var current = Interlocked.Increment(ref activeCount);
                lock (sync)
                    maximumActiveCount = Math.Max(maximumActiveCount, current);

                await Task.Delay(10, token);
                Interlocked.Decrement(ref activeCount);
            },
            maxDegreeOfParallelism: 3);

        Assert.Equal(3, maximumActiveCount);
    }

    [Fact]
    public async Task SelectConcurrentlyAsync_Preserves_Source_Order()
    {
        var results = await Enumerable.Range(0, 6).SelectConcurrentlyAsync(
            async (item, token) =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds((6 - item) * 5), token);
                return item * 2;
            },
            maxDegreeOfParallelism: 3);

        Assert.Equal([0, 2, 4, 6, 8, 10], results);
    }

    [Fact]
    public async Task Cancellation_Does_Not_Return_Successful_Partial_Results()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var operationCount = 0;

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Enumerable.Range(0, 5)
            .SelectConcurrentlyAsync(
                (item, token) =>
                {
                    operationCount++;
                    return new ValueTask<int>(item);
                },
                maxDegreeOfParallelism: 2,
                cancellationToken: cancellation.Token));

        Assert.Equal(0, operationCount);
    }

    [Fact]
    public async Task Sequential_Cancellation_Does_Not_Return_Success()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var operationCount = 0;

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Array.Empty<int>().ForEachSequentiallyAsync(
            (item, token) =>
            {
                operationCount++;
                return default;
            },
            cancellationToken: cancellation.Token));

        Assert.Equal(0, operationCount);
    }

    [Fact]
    public async Task Operation_Receives_The_Callers_Cancellation_Token()
    {
        using var cancellation = new CancellationTokenSource();
        var observedToken = default(CancellationToken);

        await new[] { 1 }.ForEachConcurrentlyAsync(
            (_, token) =>
            {
                observedToken = token;
                return default;
            },
            maxDegreeOfParallelism: 1,
            cancellationToken: cancellation.Token);

        Assert.Equal(cancellation.Token, observedToken);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Concurrent_APIs_Reject_NonPositive_Parallelism(int maxDegreeOfParallelism)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => Array.Empty<int>().ForEachConcurrentlyAsync(
            (_, _) => default,
            maxDegreeOfParallelism));
    }
}
