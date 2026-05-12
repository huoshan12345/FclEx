namespace FclEx.Utils.Consumers;

public class AutoRetryConsumerTests
{
    [Fact]
    public async Task Test()
    {
        using var consumer = new AutoRetryConsumer<int>(2);
        consumer.ConsumingHandler += (sender, _) => throw new Exception();
        var exceptions = 0;
        consumer.ExceptionHandler += (sender, args) =>
        {
            exceptions++;
            Assert.NotNull(args.Exception);
        };
        consumer.DiscardHandler += (_, args) =>
        {
            Assert.NotNull(args.Exception);
        };

        var task = consumer.StartAsync();
        // ReSharper disable once AccessToDisposedClosure
        var items = Enumerable.Range(1, 3)
            .Do(m => consumer.Add(m))
            .ToArray();
        consumer.CompleteAdding();
        await task;

        Assert.Equal(0, consumer.Count);
        Assert.Equal(0, consumer.Counter.Consume);
        Assert.Equal(exceptions, consumer.Counter.Exception);
        Assert.Equal(items.Length, consumer.Counter.Discard);
    }
}