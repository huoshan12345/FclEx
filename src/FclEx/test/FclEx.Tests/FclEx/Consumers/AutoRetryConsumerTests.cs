namespace FclEx.Consumers;

public class AutoRetryConsumerTests
{
    private readonly ITestOutputHelper _output;

    public AutoRetryConsumerTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task Test()
    {
        using var consumer = new AutoRetryConsumer<int>(2);
        consumer.ConsumingHandler += (sender, _) =>
        {
            _output.WriteLine("OnConsume: " + sender.Counter.Consume);
            throw new Exception();
        };
        var exceptions = 0;
        consumer.ExceptionHandler += (sender, args) =>
        {
            exceptions++;
            _output.WriteLine("OnException: " + sender.Counter.Exception);
            Assert.NotNull(args.Exception);
        };
        consumer.DiscardHandler += (_, args) =>
        {
            _output.WriteLine("OnDiscard");
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