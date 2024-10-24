namespace FclEx.Consumers;

public class BatchConsumerTests
{
    private readonly ITestOutputHelper _output;

    public BatchConsumerTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task Test()
    {
        using var consumer = new BatchConsumer<int>(5, TimeSpan.FromMilliseconds(100), 2);
        consumer.ConsumingHandler += (_, _) =>
        {
            _output.WriteLine("OnConsume");
            throw new Exception();
        };
        var exceptions = 0;
        consumer.ExceptionHandler += (_, args) =>
        {
            exceptions++;
            args.ForEach(m =>
            {
                _output.WriteLine("OnException");
                Assert.NotNull(m.Exception);
                Assert.IsAssignableFrom<Exception>(m.Exception);
            });
        };
        consumer.DiscardHandler += (_, args) => args.ForEach(m =>
        {
            _output.WriteLine("OnDiscard");
            Assert.NotNull(m.Exception);
            Assert.IsAssignableFrom<Exception>(m.Exception);
        });
        var task = consumer.StartAsync();
        var items = Enumerable.Range(1, 3).ToArray();

        foreach (var item in items)
        {
            consumer.Add(item);
            await Task.Delay(TimeSpan.FromMilliseconds(50));
        }

        consumer.CompleteAdding();
        await task;
        Assert.Equal(0, consumer.Count);
        Assert.Equal(0, consumer.Counter.Consume);
        Assert.Equal(exceptions, consumer.Counter.Exception);
        Assert.Equal(items.Length, consumer.Counter.Discard);
    }
}