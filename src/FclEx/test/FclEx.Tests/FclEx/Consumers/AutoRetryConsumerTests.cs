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
        const int maxRetry = 3;
        using var consumer = new AutoRetryConsumer<int>(maxRetry);
        consumer.ConsumingHandler += (sender, ints) =>
        {
            _output.WriteLine("OnConsume");
            throw new Exception();
        };
        var exceptions = 0;
        consumer.ExceptionHandler += (sender, args) =>
        {
            exceptions++;
            _output.WriteLine("OnException");
            Assert.NotNull(args.Exception);
        };
        consumer.DiscardHandler += (sender, args) =>
        {
            _output.WriteLine("OnDiscard");
            Assert.NotNull(args.Exception);
        };
        var task = consumer.Start();
        var items = Enumerable.Range(1, 3).ToArray();
        await items.ToSeriallyExecutedTask(async m =>
        {
            consumer.Add(m);
            await TaskHelper.DelayMilli(100);
        });
        consumer.CompleteAdding();
        await task;
        Assert.Equal(0, consumer.Count);
        Assert.Equal(0, consumer.Counter.Consume);
        Assert.Equal(exceptions, consumer.Counter.Exception);
        Assert.Equal(items.Length, consumer.Counter.Discard);
    }
}