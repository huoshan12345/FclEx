namespace FclEx.Extensions.EnumerableExtensions;

public class IOPairTests
{
    [Fact]
    public async Task ToOperationIOPairs_WhenAlreadyCanceled_ProducesCanceledResultForEachInput()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var invocationCount = 0;

        var pairs = await new[] { 1, 2 }.ToOperationIOPairs(
            _ =>
            {
                invocationCount++;
                return Task.FromResult(Operation.Success(1));
            },
            batchSize: 1,
            cancellation.Token);

        Assert.Equal(0, invocationCount);
        Assert.Empty(pairs.Succeeded);
        Assert.Collection(pairs.Failed,
            pair =>
            {
                Assert.Equal(1, pair.Input);
                Assert.True(pair.Output.IsCanceled());
            },
            pair =>
            {
                Assert.Equal(2, pair.Input);
                Assert.True(pair.Output.IsCanceled());
            });
    }

    [Fact]
    public async Task ToOperationIOPairsSerially_WhenAlreadyCanceled_ProducesCanceledResultForEachInput()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var invocationCount = 0;

        var pairs = await new[] { 1, 2 }.ToOperationIOPairsSerially(
            _ =>
            {
                invocationCount++;
                return Task.FromResult(Operation.Success(1));
            },
            token: cancellation.Token);

        Assert.Equal(0, invocationCount);
        Assert.Empty(pairs.Succeeded);
        Assert.All(pairs.Failed, pair => Assert.True(pair.Output.IsCanceled()));
        Assert.Equal([1, 2], pairs.Failed.Select(pair => pair.Input));
    }

    [Fact]
    public async Task ToOperationIOPairsSerially_DoesNotDelayAfterTheFinalOperation()
    {
        var operation = new[] { 1 }.ToOperationIOPairsSerially(
            _ => Task.FromResult(Operation.Success(1)),
            interval: TimeSpan.FromSeconds(1));

        var completed = await Task.WhenAny(operation, Task.Delay(TimeSpan.FromMilliseconds(200)));

        Assert.Same(operation, completed);
        Assert.Single((await operation).Succeeded);
    }
}
