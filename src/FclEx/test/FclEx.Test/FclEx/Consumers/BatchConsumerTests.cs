using System;
using System.Linq;
using System.Threading.Tasks;
using FclEx.Helpers;
using MoreLinq.Extensions;
using Xunit.Abstractions;

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
        const int maxRetry = 3;
        using var consumer = new BatchConsumer<int>(5, TimeSpan.FromSeconds(1), maxRetry);
        consumer.ConsumingHandler += (sender, ints) =>
        {
            _output.WriteLine("OnConsume");
            throw new Exception();
        };
        var exceptions = 0;
        consumer.ExceptionHandler += (sender, args) =>
        {
            exceptions++;
            args.ForEach(m =>
            {
                _output.WriteLine("OnException");
                Assert.NotNull(m.Exception);
                Assert.IsAssignableFrom<Exception>(m.Exception);
            });
        };
        consumer.DiscardHandler += (sender, args) => args.ForEach(m =>
        {
            _output.WriteLine("OnDiscard");
            Assert.NotNull(m.Exception);
            Assert.IsAssignableFrom<Exception>(m.Exception);
        });
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